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
        : IRequest<EnrollmentSuggestedCreateResponse>, ICommand;

    public sealed class Create(
        IEnrollmentRepository repository,
        IQueryBus queryBus,
        IUserProvider userProvider,
        IClock clock) : IUseCase
    {
        public async Task<EnrollmentSuggestedCreateResponse> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            if (command.ClassId == Guid.Empty || command.PreviousClassId == Guid.Empty)
            {
                throw new EnrollmentExceptions.ClassRequired();
            }

            if (command.ClassId == command.PreviousClassId)
            {
                throw new EnrollmentExceptions.SameClass();
            }

            ClassResponse newClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            ClassResponse previousClass = await queryBus.Send(new ClassFinderById.Query(command.PreviousClassId), ct);

            Guid newSchoolYearId = newClass.SchoolYearId;
            Guid previousSchoolYearId = previousClass.SchoolYearId;

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(newSchoolYearId), ct);

            bool isRepresentative = await queryBus.Send(new UserIsClassRepresentative.Query(current, command.ClassId), ct);
            if (!isRepresentative)
            {
                throw new EnrollmentExceptions.SuggestForbidden();
            }

            if (newSchoolYearId == previousSchoolYearId)
            {
                throw new EnrollmentExceptions.SameSchoolYear();
            }

            IReadOnlyCollection<SchoolYearResponse> schoolYears = await queryBus.Send(new SchoolYearFinderByIds.Query([newSchoolYearId, previousSchoolYearId]), ct);
            SchoolYearResponse? newSchoolYear = schoolYears.FirstOrDefault(schoolYear => schoolYear.Id == newSchoolYearId);
            SchoolYearResponse? previousSchoolYear = schoolYears.FirstOrDefault(schoolYear => schoolYear.Id == previousSchoolYearId);
            if (newSchoolYear is null || previousSchoolYear is null || newSchoolYear.SchoolId != previousSchoolYear.SchoolId)
            {
                throw new EnrollmentExceptions.DifferentSchool();
            }

            List<Guid> candidates = (await repository.SearchAsync(
                    new EnrollmentSpecification(classId: command.PreviousClassId, status: EnrollmentStatus.Confirmed), ct))
                .Select(enrollment => enrollment.StudentId)
                .Distinct()
                .ToList();
            if (candidates.Count == 0)
            {
                return new EnrollmentSuggestedCreateResponse(0, 0);
            }

            IReadOnlyCollection<Enrollment> inNewClass = await repository.SearchAsync(
                new EnrollmentSpecification(studentIds: candidates, classIds: [command.ClassId]), ct);

            IReadOnlyCollection<ClassResponse> newSchoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(newSchoolYearId), ct);
            IReadOnlyCollection<Enrollment> confirmedInNewSchoolYear = await repository.SearchAsync(
                new EnrollmentSpecification(
                    studentIds: candidates,
                    classIds: newSchoolYearClasses.Select(schoolClass => schoolClass.Id).ToList(),
                    status: EnrollmentStatus.Confirmed),
                ct);

            HashSet<Guid> alreadyEnrolled = inNewClass.Concat(confirmedInNewSchoolYear)
                .Select(enrollment => enrollment.StudentId)
                .ToHashSet();

            int created = 0;
            foreach (Guid studentId in candidates.Where(studentId => !alreadyEnrolled.Contains(studentId)))
            {
                Enrollment suggestion = Enrollment.CreateSuggested(Guid.NewGuid(), studentId, command.ClassId, clock);

                await repository.AddAsync(suggestion, ct);
                created++;
            }

            return new EnrollmentSuggestedCreateResponse(created, candidates.Count - created);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, EnrollmentSuggestedCreateResponse>
    {
        public Task<EnrollmentSuggestedCreateResponse> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
