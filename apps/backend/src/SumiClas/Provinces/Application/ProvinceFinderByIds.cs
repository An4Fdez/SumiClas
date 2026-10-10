using MediatR;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Provinces.Application;

public static class ProvinceFinderByIds
{
    /// <summary>Returns only the provinces that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> ProvinceIds) : IQuery<IReadOnlyCollection<ProvinceResponse>>;

    public sealed class Handler(IProvinceRepository provinceRepository) : IRequestHandler<Query, IReadOnlyCollection<ProvinceResponse>>
    {
        public async Task<IReadOnlyCollection<ProvinceResponse>> Handle(Query query, CancellationToken ct)
        {
            var provinces = await provinceRepository.SearchAsync(new ProvinceSpecification(ids: query.ProvinceIds), ct);
            return provinces
                .Select(province => new ProvinceResponse(province.Id, province.Name))
                .ToList();
        }
    }
}
