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
    public sealed record Command(
        Guid Id,
        string Scope,
        Guid? ClassId,
        string Status,
        string Description,
        string? PhotoUrl,
        bool PhotoPrivacyConfirmed) : IRequest<Result>, ICommand;

    public sealed class Create(ILostItemRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var authorId = current.Value;

            if (!Enum.TryParse<ItemScope>(command.Scope, ignoreCase: true, out var scope) || !Enum.IsDefined(scope))
            {
                return Result.Failure(LostItemErrors.InvalidScope);
            }
            if (!Enum.TryParse<ItemStatus>(command.Status, ignoreCase: true, out var status) || !Enum.IsDefined(status))
            {
                return Result.Failure(LostItemErrors.InvalidStatus);
            }

            var creation = LostItem.Create(
                command.Id,
                scope,
                command.ClassId,
                status,
                command.Description,
                command.PhotoUrl,
                command.PhotoPrivacyConfirmed,
                authorId,
                clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            if (scope == ItemScope.Class)
            {
                var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId!.Value), ct);
                if (schoolClass.IsFailure)
                {
                    return Result.Failure(schoolClass.Error!);
                }

                var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
                if (schoolYear.IsFailure)
                {
                    return schoolYear;
                }
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
