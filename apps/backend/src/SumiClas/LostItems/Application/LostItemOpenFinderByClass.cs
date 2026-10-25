using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemOpenFinderByClass
{
    /// <summary>
    /// How many lost and found items of the school and of the class have not been marked as recovered yet, whenever
    /// they were posted and whoever posted them, so the answer does not depend on who asks.
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<LostItemOpenResponse>;

    public sealed class Finder(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider)
        : IUseCase
    {
        public async Task<LostItemOpenResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            Guid school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

            int count = (await repository.SearchAsync(
                    new LostItemSpecification(scope: ItemScope.School, schoolId: school, recovered: false), ct)).Count
                + (await repository.SearchAsync(
                    new LostItemSpecification(scope: ItemScope.Class, classId: query.ClassId, recovered: false), ct)).Count;

            return new LostItemOpenResponse(count);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, LostItemOpenResponse>
    {
        public Task<LostItemOpenResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
