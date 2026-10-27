using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Announcements.Application;

public static class AnnouncementPeriodFinderByClass
{
    /// <summary>
    /// The announcements of the school and of the class published from the day <c>From</c> to the day <c>To</c>, both
    /// included (a day is a UTC date), oldest first. They carry no author and no read receipt, so the answer does not
    /// depend on who asks. A period that ends before it starts has none.
    /// </summary>
    public sealed record Query(Guid ClassId, DateOnly From, DateOnly To)
        : IQuery<IReadOnlyCollection<AnnouncementPeriodResponse>>;

    public sealed class Finder(IAnnouncementRepository repository, IQueryBus queryBus, IUserProvider userProvider)
        : IUseCase
    {
        public async Task<IReadOnlyCollection<AnnouncementPeriodResponse>> ExecuteAsync(
            Query query,
            CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            Guid school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

            DateTimeOffset from = StartOf(query.From);
            DateTimeOffset before = StartOf(query.To.AddDays(1));

            return (await repository.SearchAsync(
                    new AnnouncementSpecification(
                        scope: AnnouncementScope.School, schoolId: school, createdFrom: from, createdBefore: before), ct))
                .Concat(await repository.SearchAsync(
                    new AnnouncementSpecification(
                        scope: AnnouncementScope.Class, classId: query.ClassId, createdFrom: from, createdBefore: before), ct))
                .OrderBy(announcement => announcement.CreatedAt)
                .ThenBy(announcement => announcement.Id, Pagination.IdComparer)
                .Select(announcement => new AnnouncementPeriodResponse(
                    announcement.Title,
                    announcement.Priority == Priority.High,
                    announcement.Body))
                .ToList();
        }

        private static DateTimeOffset StartOf(DateOnly day)
            => new(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    public sealed class Handler(Finder finder)
        : IRequestHandler<Query, IReadOnlyCollection<AnnouncementPeriodResponse>>
    {
        public Task<IReadOnlyCollection<AnnouncementPeriodResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
