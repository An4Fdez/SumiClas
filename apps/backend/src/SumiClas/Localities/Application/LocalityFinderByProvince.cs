using MediatR;
using SumiClas.Localities.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Localities.Application;

public static class LocalityFinderByProvince
{
    /// <summary>
    /// Localities of the province whose name contains <paramref name="Name"/> (every locality when blank), by name.
    /// Not paginated: the largest province has under 1,000 localities.
    /// </summary>
    public sealed record Query(Guid ProvinceId, string? Name) : IQuery<IReadOnlyCollection<LocalityResponse>>;

    public sealed class Handler(ILocalityRepository localityRepository) : IRequestHandler<Query, IReadOnlyCollection<LocalityResponse>>
    {
        public async Task<IReadOnlyCollection<LocalityResponse>> Handle(Query query, CancellationToken ct)
        {
            var localities = await localityRepository.SearchAsync(
                new LocalitiesInProvinceByNameSpecification(query.ProvinceId, query.Name), ct);
            return localities
                .Select(locality => new LocalityResponse(locality.Id, locality.ProvinceId, locality.Name))
                .ToList();
        }
    }
}
