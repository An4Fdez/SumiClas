using MediatR;
using SumiClas.PostalCodes.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.PostalCodes.Application;

public static class PostalCodeFinderByIds
{
    /// <summary>Returns only the postal codes that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> PostalCodeIds) : IQuery<IReadOnlyCollection<PostalCodeResponse>>;

    public sealed class Finder(IPostalCodeRepository postalCodeRepository) : IUseCase
    {
        public async Task<IReadOnlyCollection<PostalCodeResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var postalCodes = await postalCodeRepository.SearchAsync(new PostalCodeSpecification(ids: query.PostalCodeIds), ct);
            return postalCodes
                .Select(postalCode => new PostalCodeResponse(postalCode.Id, postalCode.LocalityId, postalCode.Code))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<PostalCodeResponse>>
    {
        public Task<IReadOnlyCollection<PostalCodeResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
