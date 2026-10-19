using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.LostItems.Domain;

namespace SumiClas.LostItems.Application;

public static class LostItemFinderByClass
{
    /// <summary>School-wide lost and found items plus the ones of the class, newest first, one page at a time.</summary>
    public sealed record Query(Guid ClassId, int? Limit = null, string? Cursor = null) : IQuery<Result<PagedResponse<LostItemResponse>>>;

    public sealed class Finder(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<PagedResponse<LostItemResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var pageRequest = Pagination.Resolve(query.Limit, query.Cursor);
            if (pageRequest.IsFailure)
            {
                return Result<PagedResponse<LostItemResponse>>.Failure(pageRequest.Error!);
            }

            var page = Pagination.ByCreatedAt(pageRequest.Value);
            if (page.IsFailure)
            {
                return Result<PagedResponse<LostItemResponse>>.Failure(page.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<PagedResponse<LostItemResponse>>.Failure(schoolClass.Error!);
            }

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<PagedResponse<LostItemResponse>>.Failure(current.Error!);
            }

            var access = await queryBus.Send(new UserClassAccessEnsure.Query(current.Value, query.ClassId), ct);
            if (access.IsFailure)
            {
                return Result<PagedResponse<LostItemResponse>>.Failure(access.Error!);
            }

            var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.Value.SchoolYearId), ct);
            if (school.IsFailure)
            {
                return Result<PagedResponse<LostItemResponse>>.Failure(school.Error!);
            }

            var candidates = (await repository.SearchAsync(
                    new LostItemSpecification(scope: ItemScope.School, schoolId: school.Value, page: page.Value), ct))
                .Concat(await repository.SearchAsync(new LostItemSpecification(scope: ItemScope.Class, classId: query.ClassId, page: page.Value), ct))
                .OrderByDescending(lostItem => lostItem.CreatedAt)
                .ThenByDescending(lostItem => lostItem.Id, Pagination.IdComparer)
                .ToList();
            var (lostItems, nextCursor) = Pagination.Cut(candidates, pageRequest.Value.Limit, lostItem => Pagination.CursorOf(lostItem.CreatedAt, lostItem.Id));

            var authorIds = lostItems.Select(lostItem => lostItem.AuthorId).Distinct().ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            IReadOnlyCollection<LostItemResponse> items = lostItems
                .Select(lostItem => new LostItemResponse(
                    lostItem.Id,
                    lostItem.Scope.ToString(),
                    lostItem.ClassId,
                    lostItem.Status.ToString(),
                    lostItem.Recovered,
                    lostItem.Description,
                    lostItem.PhotoUrl,
                    lostItem.AuthorId,
                    authors.GetValueOrDefault(lostItem.AuthorId)?.FullName ?? string.Empty,
                    authors.GetValueOrDefault(lostItem.AuthorId)?.ClassRole,
                    lostItem.CreatedAt))
                .ToList();

            return Result<PagedResponse<LostItemResponse>>.Success(new PagedResponse<LostItemResponse>(items, nextCursor));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<PagedResponse<LostItemResponse>>>
    {
        public Task<Result<PagedResponse<LostItemResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
