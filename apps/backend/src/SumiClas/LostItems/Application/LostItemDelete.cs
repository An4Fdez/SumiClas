using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemDelete
{
    /// <summary>The author deletes the lost object for good.</summary>
    public sealed record Command(Guid Id) : IRequest, ICommand;

    public sealed class Delete(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
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

            if (lostItem.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            repository.Remove(lostItem);
        }
    }

    public sealed class Handler(Delete delete) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => delete.ExecuteAsync(command, ct);
    }
}
