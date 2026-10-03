using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentConfirm
{
    public sealed record Command(Guid EnrollmentId) : IRequest<Result>, ICommand;

    public sealed class UseCase(IEnrollmentRepository enrollments, IQueryBus queryBus, IClock clock)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var enrollment = await enrollments.GetByIdAsync(command.EnrollmentId, ct);
            if (enrollment is null)
            {
                return Result.Failure(EnrollmentErrors.NotFound);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(enrollment.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result.Failure(schoolClass.Error!);
            }

            var schoolYearId = schoolClass.Value.SchoolYearId;

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            var schoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct);
            var classIds = schoolYearClasses.Select(c => c.Id).ToList();
            var confirmed = await enrollments.SearchAsync(
                new EnrollmentSpecification(studentId: enrollment.StudentId, classIds: classIds, status: EnrollmentStatus.Confirmed), ct);
            if (confirmed.Any(other => other.Id != enrollment.Id))
            {
                return Result.Failure(EnrollmentErrors.AlreadyExists);
            }

            return enrollment.Confirm(clock);
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
