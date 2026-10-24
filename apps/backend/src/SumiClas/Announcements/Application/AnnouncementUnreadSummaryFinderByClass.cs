using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Announcements.Application;

public static class AnnouncementUnreadSummaryFinderByClass
{
    /// <summary>
    /// How many announcements of the school and of the class the current user has not read, among the ones of the last
    /// <c>Days</c> days (<see cref="SummaryPeriod.DefaultDays"/> when not given), and how many of those are high priority.
    /// The ones the user published are not news to them, so they are left out.
    /// </summary>
    public sealed record Query(Guid ClassId, int? Days = null) : IQuery<AnnouncementUnreadSummaryResponse>;

    public sealed class Finder(IAnnouncementRepository repository, IQueryBus queryBus, IUserProvider userProvider, IClock clock)
        : IUseCase
    {
        public async Task<AnnouncementUnreadSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            int days = SummaryPeriod.Resolve(query.Days);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            Guid school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

            DateTimeOffset from = clock.UtcNow.AddDays(-days);

            List<Announcement> unread = (await repository.SearchAsync(
                    new AnnouncementSpecification(scope: AnnouncementScope.School, schoolId: school, createdFrom: from, unreadByUserId: current), ct))
                .Concat(await repository.SearchAsync(
                    new AnnouncementSpecification(scope: AnnouncementScope.Class, classId: query.ClassId, createdFrom: from, unreadByUserId: current), ct))
                .Where(announcement => announcement.AuthorId != current)
                .ToList();

            return new AnnouncementUnreadSummaryResponse(
                unread.Count,
                unread.Count(announcement => announcement.Priority == Priority.High));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, AnnouncementUnreadSummaryResponse>
    {
        public Task<AnnouncementUnreadSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
