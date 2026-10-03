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
    /// <summary>School-wide announcements plus the ones of the class, newest first.</summary>
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<AnnouncementResponse>>>;

    public sealed class Handler(IAnnouncementRepository announcementRepository, IQueryBus queryBus, IUserProvider userProvider)
        : IRequestHandler<Query, Result<IReadOnlyCollection<AnnouncementResponse>>>
    {
        public async Task<Result<IReadOnlyCollection<AnnouncementResponse>>> Handle(Query query, CancellationToken ct)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<IReadOnlyCollection<AnnouncementResponse>>.Failure(current.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<AnnouncementResponse>>.Failure(schoolClass.Error!);
            }

            var announcements = (await announcementRepository.SearchAsync(new AnnouncementSpecification(scope: AnnouncementScope.School), ct))
                .Concat(await announcementRepository.SearchAsync(new AnnouncementSpecification(scope: AnnouncementScope.Class, classId: query.ClassId), ct))
                .ToList();

            var authorIds = announcements.Select(announcement => announcement.AuthorId).Distinct().ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            var eventIds = announcements.Where(announcement => announcement.RelatedEventId is not null)
                .Select(announcement => announcement.RelatedEventId!.Value).Distinct().ToList();
            var events = (await queryBus.Send(new CalendarEventFinderByIds.Query(eventIds), ct))
                .ToDictionary(calendarEvent => calendarEvent.Id);

            IReadOnlyCollection<AnnouncementResponse> response = announcements
                .OrderByDescending(announcement => announcement.CreatedAt)
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

            return Result<IReadOnlyCollection<AnnouncementResponse>>.Success(response);
        }
    }
}
