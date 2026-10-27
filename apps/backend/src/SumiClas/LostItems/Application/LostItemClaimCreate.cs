using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemClaimCreate
{
    /// <summary>A class member other than the author offers to resolve the item, with the detail that proves it.</summary>
    public sealed record Command(Guid Id, Guid LostItemId, string Message) : IRequest, ICommand;

    public sealed class Create(ILostItemRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            LostItem lostItem = await repository.GetByIdAsync(command.LostItemId, ct)
                ?? throw new LostItemExceptions.NotFound();

            // A school-wide item has no class to check the membership against, and publishing at school level is closed.
            Guid classId = lostItem.ClassId ?? throw new LostItemExceptions.ClaimClassRequired();

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, classId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            lostItem.AddClaim(command.Id, current, command.Message, clock);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
