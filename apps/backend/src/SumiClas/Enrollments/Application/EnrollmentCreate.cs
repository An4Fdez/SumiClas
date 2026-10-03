using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentCreate
{
    public sealed record Command(Guid Id, Guid StudentId, Guid ClassId) : IRequest<Result>, ICommand;

    public sealed class UseCase(IEnrollmentRepository enrollments, IQueryBus queryBus, IClock clock)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var creation = Enrollment.Create(command.Id, command.StudentId, command.ClassId, clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var student = await queryBus.Send(new StudentFinderById.Query(command.StudentId), ct);
            if (student.IsFailure)
            {
                return Result.Failure(student.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);
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
                new EnrollmentSpecification(studentId: command.StudentId, classIds: classIds, status: EnrollmentStatus.Confirmed), ct);
            if (confirmed.Count > 0)
            {
                return Result.Failure(EnrollmentErrors.AlreadyExists);
            }

            await enrollments.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
