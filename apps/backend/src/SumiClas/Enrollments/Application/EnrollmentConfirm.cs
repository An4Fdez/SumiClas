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
    public sealed record Command(Guid EnrollmentId) : IRequest, ICommand;

    public sealed class Confirm(IEnrollmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Enrollment? enrollment = await repository.GetByIdAsync(command.EnrollmentId, ct);
            if (enrollment is null)
            {
                throw new EnrollmentExceptions.NotFound();
            }

            await queryBus.Send(new StudentGuardianEnsure.Query(enrollment.StudentId, current), ct);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(enrollment.ClassId), ct);

            Guid schoolYearId = schoolClass.SchoolYearId;

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolYearId), ct);

            IReadOnlyCollection<ClassResponse> schoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct);
            List<Guid> classIds = schoolYearClasses.Select(c => c.Id).ToList();
            IReadOnlyCollection<Enrollment> confirmed = await repository.SearchAsync(
                new EnrollmentSpecification(studentId: enrollment.StudentId, classIds: classIds, status: EnrollmentStatus.Confirmed), ct);
            if (confirmed.Any(other => other.Id != enrollment.Id))
            {
                throw new EnrollmentExceptions.AlreadyExists();
            }

            enrollment.Confirm(clock);
        }
    }

    public sealed class Handler(Confirm confirm) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => confirm.ExecuteAsync(command, ct);
    }
}
