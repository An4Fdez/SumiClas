using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Schools.Domain;

namespace SumiClas.Schools.Application;

public static class SchoolFinderByIds
{
    /// <summary>Returns only the schools that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> SchoolIds) : IQuery<IReadOnlyCollection<SchoolResponse>>;

    public sealed class Handler(ISchoolRepository schoolRepository) : IRequestHandler<Query, IReadOnlyCollection<SchoolResponse>>
    {
        public async Task<IReadOnlyCollection<SchoolResponse>> Handle(Query query, CancellationToken ct)
        {
            var schools = await schoolRepository.SearchAsync(new SchoolSpecification(ids: query.SchoolIds), ct);
            return schools
                .Select(school => new SchoolResponse(school.Id, school.Name, school.OfficialCode, school.Address))
                .ToList();
        }
    }
}
