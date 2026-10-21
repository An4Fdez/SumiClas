using MediatR;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Application;

public static class ClassCreate
{
    /// <summary>The current user creates the class and becomes its first delegate, through the ClassCreatedDomainEvent.</summary>
    public sealed record Command(Guid Id, Guid SchoolYearId, string Name) : IRequest, ICommand;

    public sealed class Create(IClassRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId().OrThrow();

            var schoolClass = Class.Create(command.Id, command.SchoolYearId, command.Name, current, clock);

            (await queryBus.Send(new SchoolYearEnsureIsActive.Query(command.SchoolYearId), ct)).OrThrow();

            await repository.AddAsync(schoolClass, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
