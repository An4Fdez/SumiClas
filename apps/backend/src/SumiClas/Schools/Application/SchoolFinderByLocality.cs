using MediatR;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Schools.Application;

public static class SchoolFinderByLocality
{
    public const int MaxResults = 50;

    /// <summary>Schools of the locality whose name contains <paramref name="Name"/> (every school when blank), by name, at most <see cref="MaxResults"/>. A missing or empty <paramref name="LocalityId"/> is rejected.</summary>
    public sealed record Query(Guid? LocalityId, string? Name) : IQuery<IReadOnlyCollection<SchoolResponse>>;

    public sealed class Finder(ISchoolRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<IReadOnlyCollection<SchoolResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            if (query.LocalityId is null || query.LocalityId == Guid.Empty)
            {
                throw new SchoolExceptions.LocalityFilterRequired();
            }

            IReadOnlyCollection<School> schools = await repository.SearchAsync(
                new SchoolsInLocalityByNameSpecification(query.LocalityId.Value, query.Name, MaxResults), ct);
            return await SchoolResponseFactory.CreateAsync(queryBus, schools, ct);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<SchoolResponse>>
    {
        public Task<IReadOnlyCollection<SchoolResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
