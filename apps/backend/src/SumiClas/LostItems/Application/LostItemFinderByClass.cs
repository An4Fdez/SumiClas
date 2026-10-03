using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.LostItems.Domain;

namespace SumiClas.LostItems.Application;

public static class LostItemFinderByClass
{
    /// <summary>School-wide lost and found items plus the ones of the class, newest first.</summary>
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<LostItemResponse>>>;

    public sealed class Handler(ILostItemRepository lostItemRepository, IQueryBus queryBus)
        : IRequestHandler<Query, Result<IReadOnlyCollection<LostItemResponse>>>
    {
        public async Task<Result<IReadOnlyCollection<LostItemResponse>>> Handle(Query query, CancellationToken ct)
        {
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<LostItemResponse>>.Failure(schoolClass.Error!);
            }

            var lostItems = (await lostItemRepository.SearchAsync(new LostItemSpecification(scope: ItemScope.School), ct))
                .Concat(await lostItemRepository.SearchAsync(new LostItemSpecification(scope: ItemScope.Class, classId: query.ClassId), ct))
                .ToList();

            var authorIds = lostItems.Select(lostItem => lostItem.AuthorId).Distinct().ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            IReadOnlyCollection<LostItemResponse> response = lostItems
                .OrderByDescending(lostItem => lostItem.CreatedAt)
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

            return Result<IReadOnlyCollection<LostItemResponse>>.Success(response);
        }
    }
}
