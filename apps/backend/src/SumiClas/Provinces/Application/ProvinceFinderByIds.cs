using MediatR;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Provinces.Application;

public static class ProvinceFinderByIds
{
    /// <summary>Returns only the provinces that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> ProvinceIds) : IQuery<IReadOnlyCollection<ProvinceResponse>>;

    public sealed class Finder(IProvinceRepository repository) : IUseCase
    {
        public async Task<IReadOnlyCollection<ProvinceResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var provinces = await repository.SearchAsync(new ProvinceSpecification(ids: query.ProvinceIds), ct);
            return provinces
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
