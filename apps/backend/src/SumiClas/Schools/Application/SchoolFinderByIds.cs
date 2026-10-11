using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Schools.Domain;

namespace SumiClas.Schools.Application;

public static class SchoolFinderByIds
{
    /// <summary>Returns only the schools that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> SchoolIds) : IQuery<IReadOnlyCollection<SchoolResponse>>;

    public sealed class Finder(ISchoolRepository schoolRepository, IQueryBus queryBus) : IUseCase
    {
        public async Task<IReadOnlyCollection<SchoolResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var schools = await schoolRepository.SearchAsync(new SchoolSpecification(ids: query.SchoolIds), ct);
            return await SchoolResponseFactory.CreateAsync(queryBus, schools, ct);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<SchoolResponse>>
    {
        public Task<IReadOnlyCollection<SchoolResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
