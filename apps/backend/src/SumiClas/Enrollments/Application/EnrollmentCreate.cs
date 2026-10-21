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
    public sealed record Command(Guid Id, Guid StudentId, Guid ClassId) : IRequest<Result>, ICommand;

    public sealed class Create(IEnrollmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var creation = Enrollment.Create(command.Id, command.StudentId, command.ClassId, clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var guardian = await queryBus.Send(new StudentGuardianEnsure.Query(command.StudentId, current.Value), ct);
            if (guardian.IsFailure)
            {
                return guardian;
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            var schoolYearId = schoolClass.SchoolYearId;

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolYearId), ct);

            var schoolYearClasses = await queryBus.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct);
            var classIds = schoolYearClasses.Select(c => c.Id).ToList();
            var confirmed = await repository.SearchAsync(
                new EnrollmentSpecification(studentId: command.StudentId, classIds: classIds, status: EnrollmentStatus.Confirmed), ct);
            if (confirmed.Count > 0)
            {
                return Result.Failure(EnrollmentErrors.AlreadyExists);
            }

            await repository.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
