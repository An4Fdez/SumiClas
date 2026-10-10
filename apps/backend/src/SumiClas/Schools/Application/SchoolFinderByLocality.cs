using MediatR;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Schools.Application;

public static class SchoolFinderByLocality
{
    public const int MaxResults = 50;

    /// <summary>Schools of the locality whose name contains <paramref name="Name"/> (every school when blank), by name, at most <see cref="MaxResults"/>.</summary>
    public sealed record Query(Guid LocalityId, string? Name) : IQuery<IReadOnlyCollection<SchoolResponse>>;

    public sealed class Handler(ISchoolRepository schoolRepository) : IRequestHandler<Query, IReadOnlyCollection<SchoolResponse>>
    {
        public async Task<IReadOnlyCollection<SchoolResponse>> Handle(Query query, CancellationToken ct)
        {
            var schools = await schoolRepository.SearchAsync(
                new SchoolsInLocalityByNameSpecification(query.LocalityId, query.Name), ct);
            return schools
                .OrderBy(school => school.Name, StringComparer.Ordinal)
                .Take(MaxResults)
                .Select(school => new SchoolResponse(
                    school.Id, school.Name, school.OfficialCode, school.LocalityId, school.PostalCodeId, school.Address, school.Phone))
                .ToList();
        }
    }
}
