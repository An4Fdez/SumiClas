using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemCreate
{
    /// <summary>
    /// ClassId is always the class the lost item is published from, and the author must belong to it. For a
    /// school-wide lost item the class only gives the school the item belongs to; it is not stored.
    /// </summary>
    public sealed record Command(
        Guid Id,
        string Scope,
        Guid? ClassId,
        string Status,
        string Description,
        string? PhotoUrl,
        bool PhotoPrivacyConfirmed) : IRequest, ICommand;

    public sealed class Create(ILostItemRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var authorId = current;

            if (!Enum.TryParse<ItemScope>(command.Scope, ignoreCase: true, out var scope) || !Enum.IsDefined(scope))
            {
                throw new LostItemExceptions.InvalidScope();
            }
            if (!Enum.TryParse<ItemStatus>(command.Status, ignoreCase: true, out var status) || !Enum.IsDefined(status))
            {
                throw new LostItemExceptions.InvalidStatus();
            }

            // The school of a school-wide lost item is only known after looking up the class, so the aggregate is
            // built twice: once only to validate the input before any IO (the school is provisional and discarded),
            // and once with the real school.
            var lostItem = LostItem.Create(
                command.Id,
                scope,
                command.ClassId,
                scope == ItemScope.School ? Guid.NewGuid() : null,
                status,
                command.Description,
                command.PhotoUrl,
                command.PhotoPrivacyConfirmed,
                authorId,
                clock);

            if (command.ClassId is not { } classId || classId == Guid.Empty)
            {
                throw new LostItemExceptions.ClassRequiredForSchoolScope();
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(authorId, classId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            if (scope == ItemScope.School)
            {
                var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

                lostItem = LostItem.Create(
                    command.Id,
                    scope,
                    command.ClassId,
                    school,
                    status,
                    command.Description,
                    command.PhotoUrl,
                    command.PhotoPrivacyConfirmed,
                    authorId,
                    clock);
            }

            await repository.AddAsync(lostItem, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
