using MediatR;
using SumiClas.CalendarEvents.Application;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Announcements.Domain;

namespace SumiClas.Announcements.Application;

public static class AnnouncementFinderByClass
{
    /// <summary>School-wide announcements plus the ones of the class, newest first, one page at a time.</summary>
    public sealed record Query(Guid ClassId, int? Limit = null, string? Cursor = null) : IQuery<PagedResponse<AnnouncementResponse>>;

    public sealed class Finder(IAnnouncementRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<PagedResponse<AnnouncementResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            PageRequest pageRequest = Pagination.Resolve(query.Limit, query.Cursor);

            KeysetPage<DateTimeOffset> page = Pagination.ByCreatedAt(pageRequest);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            Guid school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

            List<Announcement> candidates = (await repository.SearchAsync(
                    new AnnouncementSpecification(scope: AnnouncementScope.School, schoolId: school, page: page), ct))
                .Concat(await repository.SearchAsync(new AnnouncementSpecification(scope: AnnouncementScope.Class, classId: query.ClassId, page: page), ct))
                .OrderByDescending(announcement => announcement.CreatedAt)
                .ThenByDescending(announcement => announcement.Id, Pagination.IdComparer)
                .ToList();
            (IReadOnlyList<Announcement> announcements, string? nextCursor) = Pagination.Cut(candidates, pageRequest.Limit, announcement => Pagination.CursorOf(announcement.CreatedAt, announcement.Id));

            List<Guid> authorIds = announcements.Select(announcement => announcement.AuthorId).Distinct().ToList();
            Dictionary<Guid, UserClassMemberResponse> authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            List<Guid> eventIds = announcements.Where(announcement => announcement.RelatedEventId is not null)
                .Select(announcement => announcement.RelatedEventId!.Value).Distinct().ToList();
            Dictionary<Guid, CalendarEventSummaryResponse> events = (await queryBus.Send(new CalendarEventFinderByIds.Query(eventIds), ct))
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
                    announcement.UpdatedAt,
                    announcement.RelatedEventId is { } relatedEventId ? events.GetValueOrDefault(relatedEventId) : null,
                    announcement.ReadReceipts.Any(receipt => receipt.UserId == current),
                    announcement.AuthorId == current))
                .ToList();

            return new PagedResponse<AnnouncementResponse>(items, nextCursor);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, PagedResponse<AnnouncementResponse>>
    {
        public Task<PagedResponse<AnnouncementResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
