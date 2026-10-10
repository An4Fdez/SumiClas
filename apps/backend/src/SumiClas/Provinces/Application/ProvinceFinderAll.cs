using MediatR;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Provinces.Application;

public static class ProvinceFinderAll
{
    /// <summary>Every province, by name. There are only 52, so there is no pagination.</summary>
    public sealed record Query : IQuery<IReadOnlyCollection<ProvinceResponse>>;

    public sealed class Handler(IProvinceRepository provinceRepository) : IRequestHandler<Query, IReadOnlyCollection<ProvinceResponse>>
    {
        public async Task<IReadOnlyCollection<ProvinceResponse>> Handle(Query query, CancellationToken ct)
        {
            var provinces = await provinceRepository.SearchAsync(new ProvinceSpecification(), ct);
            return provinces
                .OrderBy(province => province.NormalizedName, StringComparer.Ordinal)
                .Select(province => new ProvinceResponse(province.Id, province.Name))
                .ToList();
        }
    }
}
