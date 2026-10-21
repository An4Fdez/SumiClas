using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Users.Application;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentConfirm
{
    public sealed record Command(Guid EnrollmentId) : IRequest<Result>, ICommand;

    public sealed class Confirm(IEnrollmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var enrollment = await repository.GetByIdAsync(command.EnrollmentId, ct);
            if (enrollment is null)
            {
                return Result.Failure(EnrollmentErrors.NotFound);
            }

            var current = userProvider.GetCurrentUserId();

            await queryBus.Send(new StudentGuardianEnsure.Query(enrollment.StudentId, current), ct);

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(enrollment.ClassId), ct);

            var schoolYearId = schoolClass.SchoolYearId;

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolYearId), ct);

            var schoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct);
            var classIds = schoolYearClasses.Select(c => c.Id).ToList();
            var confirmed = await repository.SearchAsync(
                new EnrollmentSpecification(studentId: enrollment.StudentId, classIds: classIds, status: EnrollmentStatus.Confirmed), ct);
            if (confirmed.Any(other => other.Id != enrollment.Id))
            {
                return Result.Failure(EnrollmentErrors.AlreadyExists);
            }

            return enrollment.Confirm(clock);
        }
    }

    public sealed class Handler(Confirm confirm) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => confirm.ExecuteAsync(command, ct);
    }
}
