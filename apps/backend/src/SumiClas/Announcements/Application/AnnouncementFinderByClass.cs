using MediatR;
using SumiClas.CalendarEvents.Application;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Announcements.Domain;

namespace SumiClas.Announcements.Application;

public static class AnnouncementFinderByClass
{
    /// <summary>School-wide announcements plus the ones of the class, newest first, one page at a time.</summary>
    public sealed record Query(Guid ClassId, int? Limit = null, string? Cursor = null) : IQuery<Result<PagedResponse<AnnouncementResponse>>>;

    public sealed class Finder(IAnnouncementRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<PagedResponse<AnnouncementResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<PagedResponse<AnnouncementResponse>>.Failure(current.Error!);
            }

            var pageRequest = Pagination.Resolve(query.Limit, query.Cursor);
            if (pageRequest.IsFailure)
            {
                return Result<PagedResponse<AnnouncementResponse>>.Failure(pageRequest.Error!);
            }

            var page = Pagination.ByCreatedAt(pageRequest.Value);
            if (page.IsFailure)
            {
                return Result<PagedResponse<AnnouncementResponse>>.Failure(page.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<PagedResponse<AnnouncementResponse>>.Failure(schoolClass.Error!);
            }

            var candidates = (await repository.SearchAsync(new AnnouncementSpecification(scope: AnnouncementScope.School, page: page.Value), ct))
                .Concat(await repository.SearchAsync(new AnnouncementSpecification(scope: AnnouncementScope.Class, classId: query.ClassId, page: page.Value), ct))
                .OrderByDescending(announcement => announcement.CreatedAt)
                .ThenByDescending(announcement => announcement.Id, Pagination.IdComparer)
                .ToList();
            var (announcements, nextCursor) = Pagination.Cut(candidates, pageRequest.Value.Limit, announcement => Pagination.CursorOf(announcement.CreatedAt, announcement.Id));

            var authorIds = announcements.Select(announcement => announcement.AuthorId).Distinct().ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            var eventIds = announcements.Where(announcement => announcement.RelatedEventId is not null)
                .Select(announcement => announcement.RelatedEventId!.Value).Distinct().ToList();
            var events = (await queryBus.Send(new CalendarEventFinderByIds.Query(eventIds), ct))
                .ToDictionary(calendarEvent => calendarEvent.Id);

            IReadOnlyCollection<AnnouncementResponse> items = announcements
                .Select(announcement => new AnnouncementResponse(
                    announcement.Id,
                    announcement.Scope.ToString(),
                    announcement.ClassId,
                    announcement.Title,
                    announcement.Body,
                    announcement.Priority.ToString(),
                    announcement.AuthorId,
                    authors.GetValueOrDefault(announcement.AuthorId)?.FullName ?? string.Empty,
                    authors.GetValueOrDefault(announcement.AuthorId)?.ClassRole,
                    announcement.CreatedAt,
                    announcement.RelatedEventId is { } relatedEventId ? events.GetValueOrDefault(relatedEventId) : null,
                    announcement.ReadReceipts.Any(receipt => receipt.UserId == current.Value)))
                .ToList();

            return Result<PagedResponse<AnnouncementResponse>>.Success(new PagedResponse<AnnouncementResponse>(items, nextCursor));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<PagedResponse<AnnouncementResponse>>>
    {
        public Task<Result<PagedResponse<AnnouncementResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
