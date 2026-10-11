using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentSuggestedCreate
{
    /// <summary>Suggests, in <paramref name="ClassId"/> (the new class), every student with a confirmed enrollment in <paramref name="PreviousClassId"/>.</summary>
    public sealed record Command(Guid ClassId, Guid PreviousClassId)
        : IRequest<Result<EnrollmentSuggestedCreateResponse>>, ICommand;

    public sealed class Create(
        IEnrollmentRepository enrollments,
        IQueryBus queryBus,
        IUserProvider userProvider,
        IClock clock) : IUseCase
    {
        public async Task<Result<EnrollmentSuggestedCreateResponse>> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(current.Error!);
            }

            if (command.ClassId == Guid.Empty || command.PreviousClassId == Guid.Empty)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(EnrollmentErrors.ClassRequired);
            }

            if (command.ClassId == command.PreviousClassId)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(EnrollmentErrors.SameClass);
            }

            var newClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);
            if (newClass.IsFailure)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(newClass.Error!);
            }

            var previousClass = await queryBus.Send(new ClassFinderById.Query(command.PreviousClassId), ct);
            if (previousClass.IsFailure)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(previousClass.Error!);
            }

            var newSchoolYearId = newClass.Value.SchoolYearId;
            var previousSchoolYearId = previousClass.Value.SchoolYearId;

            var active = await queryBus.Send(new SchoolYearEnsureIsActive.Query(newSchoolYearId), ct);
            if (active.IsFailure)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(active.Error!);
            }

            var isRepresentative = await queryBus.Send(new UserIsClassRepresentative.Query(current.Value, command.ClassId), ct);
            if (!isRepresentative)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(EnrollmentErrors.SuggestForbidden);
            }

            if (newSchoolYearId == previousSchoolYearId)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(EnrollmentErrors.SameSchoolYear);
            }

            var schoolYears = await queryBus.Send(new SchoolYearFinderByIds.Query([newSchoolYearId, previousSchoolYearId]), ct);
            var newSchoolYear = schoolYears.FirstOrDefault(schoolYear => schoolYear.Id == newSchoolYearId);
            var previousSchoolYear = schoolYears.FirstOrDefault(schoolYear => schoolYear.Id == previousSchoolYearId);
            if (newSchoolYear is null || previousSchoolYear is null || newSchoolYear.SchoolId != previousSchoolYear.SchoolId)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Failure(EnrollmentErrors.DifferentSchool);
            }

            var candidates = (await enrollments.SearchAsync(
                    new EnrollmentSpecification(classId: command.PreviousClassId, status: EnrollmentStatus.Confirmed), ct))
                .Select(enrollment => enrollment.StudentId)
                .Distinct()
                .ToList();
            if (candidates.Count == 0)
            {
                return Result<EnrollmentSuggestedCreateResponse>.Success(new EnrollmentSuggestedCreateResponse(0, 0));
            }

            var inNewClass = await enrollments.SearchAsync(
                new EnrollmentSpecification(studentIds: candidates, classIds: [command.ClassId]), ct);

            var newSchoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(newSchoolYearId), ct);
            var confirmedInNewSchoolYear = await enrollments.SearchAsync(
                new EnrollmentSpecification(
                    studentIds: candidates,
                    classIds: newSchoolYearClasses.Select(schoolClass => schoolClass.Id).ToList(),
                    status: EnrollmentStatus.Confirmed),
                ct);

            var alreadyEnrolled = inNewClass.Concat(confirmedInNewSchoolYear)
                .Select(enrollment => enrollment.StudentId)
                .ToHashSet();

            var created = 0;
            foreach (var studentId in candidates.Where(studentId => !alreadyEnrolled.Contains(studentId)))
            {
                var suggestion = Enrollment.CreateSuggested(Guid.NewGuid(), studentId, command.ClassId, clock);
                if (suggestion.IsFailure)
                {
                    return Result<EnrollmentSuggestedCreateResponse>.Failure(suggestion.Error!);
                }

                await enrollments.AddAsync(suggestion.Value, ct);
                created++;
            }

            return Result<EnrollmentSuggestedCreateResponse>.Success(
                new EnrollmentSuggestedCreateResponse(created, candidates.Count - created));
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result<EnrollmentSuggestedCreateResponse>>
    {
        public Task<Result<EnrollmentSuggestedCreateResponse>> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
