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

            // The school of a school-wide lost item is only known after looking up the class, so the aggregate is
            // built twice: once only to validate the input before any IO (the school is provisional and discarded),
            // and once with the real school.
            var validation = LostItem.Create(
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
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error!);
            }

            if (command.ClassId is not { } classId || classId == Guid.Empty)
            {
                return Result.Failure(LostItemErrors.ClassRequiredForSchoolScope);
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);
            if (schoolClass.IsFailure)
            {
                return Result.Failure(schoolClass.Error!);
            }

            var access = await queryBus.Send(new UserClassAccessEnsure.Query(authorId, classId), ct);
            if (access.IsFailure)
            {
                return access;
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            var lostItem = validation.Value;
            if (scope == ItemScope.School)
            {
                var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.Value.SchoolYearId), ct);
                if (school.IsFailure)
                {
                    return Result.Failure(school.Error!);
                }

                var creation = LostItem.Create(
                    command.Id,
                    scope,
                    command.ClassId,
                    school.Value,
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

                lostItem = creation.Value;
            }

            await repository.AddAsync(lostItem, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
