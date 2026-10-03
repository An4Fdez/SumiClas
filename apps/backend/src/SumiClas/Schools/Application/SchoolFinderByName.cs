using MediatR;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Schools.Application;

public static class SchoolFinderByName
{
    public const int MaxResults = 50;

    /// <summary>Schools whose name contains <paramref name="Name"/> (every school when blank), by name, at most <see cref="MaxResults"/>.</summary>
    public sealed record Query(string? Name) : IQuery<IReadOnlyCollection<SchoolResponse>>;

    public sealed class Handler(ISchoolRepository schoolRepository) : IRequestHandler<Query, IReadOnlyCollection<SchoolResponse>>
    {
        public async Task<IReadOnlyCollection<SchoolResponse>> Handle(Query query, CancellationToken ct)
        {
            var schools = string.IsNullOrWhiteSpace(query.Name)
                ? await schoolRepository.SearchAsync(new SchoolSpecification(), ct)
                : await schoolRepository.SearchAsync(new SchoolsByNameContainingSpecification(query.Name), ct);
            return schools
                .OrderBy(school => school.Name)
                .Take(MaxResults)
                .Select(school => new SchoolResponse(school.Id, school.Name, school.OfficialCode, school.Address))
                .ToList();
        }
    }
}
