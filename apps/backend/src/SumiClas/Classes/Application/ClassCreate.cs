using MediatR;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Application;

public static class ClassCreate
{
    /// <summary>The current user creates the class and becomes its first delegate, through the ClassCreatedDomainEvent.</summary>
    public sealed record Command(Guid Id, Guid SchoolYearId, string Name) : IRequest<Result>, ICommand;

    public sealed class UseCase(IClassRepository classes, IQueryBus queryBus, IClock clock, IUserProvider userProvider)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var creation = Class.Create(command.Id, command.SchoolYearId, command.Name, current.Value, clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(command.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            await classes.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
