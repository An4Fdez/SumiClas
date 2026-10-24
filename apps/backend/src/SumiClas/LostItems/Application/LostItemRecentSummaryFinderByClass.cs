using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemRecentSummaryFinderByClass
{
    /// <summary>
    /// How many lost and found items of the school and of the class, posted in the last <c>Days</c> days
    /// (<see cref="SummaryPeriod.DefaultDays"/> when not given), have not been marked as recovered yet.
    /// The ones the current user posted are not news to them, so they are left out.
    /// </summary>
    public sealed record Query(Guid ClassId, int? Days = null) : IQuery<LostItemRecentSummaryResponse>;

    public sealed class Finder(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider, IClock clock)
        : IUseCase
    {
        public async Task<LostItemRecentSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            int days = SummaryPeriod.Resolve(query.Days);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            Guid school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

            DateTimeOffset from = clock.UtcNow.AddDays(-days);

            int count = (await repository.SearchAsync(
                    new LostItemSpecification(scope: ItemScope.School, schoolId: school, recovered: false, createdFrom: from), ct))
                .Concat(await repository.SearchAsync(
                    new LostItemSpecification(scope: ItemScope.Class, classId: query.ClassId, recovered: false, createdFrom: from), ct))
                .Count(lostItem => lostItem.AuthorId != current);

            return new LostItemRecentSummaryResponse(count);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, LostItemRecentSummaryResponse>
    {
        public Task<LostItemRecentSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
