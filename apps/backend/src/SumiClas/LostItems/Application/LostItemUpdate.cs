using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Application;

public static class LostItemUpdate
{
    /// <summary>
    /// The author changes the description. The status (lost or found), the photo and whether the object was recovered
    /// stay as they were: resolving it is what the recovered mark is for.
    /// </summary>
    public sealed record Command(Guid Id, string Description) : IRequest, ICommand;

    public sealed class Update(ILostItemRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            LostItem lostItem = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new LostItemExceptions.NotFound();

            if (lostItem.AuthorId != current)
            {
                throw new LostItemExceptions.ChangeForbidden();
            }

            LostItem.ValidateContent(command.Description);

            if (lostItem.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            lostItem.Update(command.Description, clock);
        }
    }

    public sealed class Handler(Update update) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => update.ExecuteAsync(command, ct);
    }
}
