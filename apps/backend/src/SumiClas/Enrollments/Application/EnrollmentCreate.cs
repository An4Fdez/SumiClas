using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Users.Application;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentCreate
{
    public sealed record Command(Guid Id, Guid StudentId, Guid ClassId) : IRequest, ICommand;

    public sealed class Create(IEnrollmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var enrollment = Enrollment.Create(command.Id, command.StudentId, command.ClassId, clock);

            var current = userProvider.GetCurrentUserId();

            await queryBus.Send(new StudentGuardianEnsure.Query(command.StudentId, current), ct);

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            var schoolYearId = schoolClass.SchoolYearId;

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolYearId), ct);

            var schoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct);
            var classIds = schoolYearClasses.Select(c => c.Id).ToList();
            var confirmed = await repository.SearchAsync(
                new EnrollmentSpecification(studentId: command.StudentId, classIds: classIds, status: EnrollmentStatus.Confirmed), ct);
            if (confirmed.Count > 0)
            {
                throw new EnrollmentExceptions.AlreadyExists();
            }

            await repository.AddAsync(enrollment, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
