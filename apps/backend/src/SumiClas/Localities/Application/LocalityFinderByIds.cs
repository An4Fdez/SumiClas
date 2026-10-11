using MediatR;
using SumiClas.Localities.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Localities.Application;

public static class LocalityFinderByIds
{
    /// <summary>Returns only the localities that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> LocalityIds) : IQuery<IReadOnlyCollection<LocalityResponse>>;

    public sealed class Finder(ILocalityRepository localityRepository) : IUseCase
    {
        public async Task<IReadOnlyCollection<LocalityResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var localities = await localityRepository.SearchAsync(new LocalitySpecification(ids: query.LocalityIds), ct);
            return localities
                .Select(locality => new LocalityResponse(locality.Id, locality.ProvinceId, locality.Name))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<LocalityResponse>>
    {
        public Task<IReadOnlyCollection<LocalityResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
