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
            Guid current = userProvider.GetCurrentUserId();

            Enrollment enrollment = Enrollment.Create(command.Id, command.StudentId, command.ClassId, clock);

            await queryBus.Send(new StudentGuardianEnsure.Query(command.StudentId, current), ct);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            Guid schoolYearId = schoolClass.SchoolYearId;

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolYearId), ct);

            IReadOnlyCollection<ClassResponse> schoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct);
            List<Guid> classIds = schoolYearClasses.Select(c => c.Id).ToList();
            IReadOnlyCollection<Enrollment> confirmed = await repository.SearchAsync(
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
