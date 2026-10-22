using MediatR;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Provinces.Application;

public static class ProvinceFinderAll
{
    /// <summary>Every province, by name. There are only 52, so there is no pagination.</summary>
    public sealed record Query : IQuery<IReadOnlyCollection<ProvinceResponse>>;

    public sealed class Finder(IProvinceRepository repository) : IUseCase
    {
        public async Task<IReadOnlyCollection<ProvinceResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            IReadOnlyCollection<Province> provinces = await repository.SearchAsync(new ProvinceSpecification(), ct);
            return provinces
                .OrderBy(province => province.NormalizedName, StringComparer.Ordinal)
                .Select(province => new ProvinceResponse(province.Id, province.Name))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<ProvinceResponse>>
    {
        public Task<IReadOnlyCollection<ProvinceResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
