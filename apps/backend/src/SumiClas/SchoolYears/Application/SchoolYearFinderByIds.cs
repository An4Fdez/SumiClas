using MediatR;
using SumiClas.Shared.Application;
using SumiClas.SchoolYears.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearFinderByIds
{
    /// <summary>Returns only the school years that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> SchoolYearIds) : IQuery<IReadOnlyCollection<SchoolYearResponse>>;

    public sealed class Handler(ISchoolYearRepository schoolYearRepository) : IRequestHandler<Query, IReadOnlyCollection<SchoolYearResponse>>
    {
        public async Task<IReadOnlyCollection<SchoolYearResponse>> Handle(Query query, CancellationToken ct)
        {
            var schoolYears = await schoolYearRepository.SearchAsync(new SchoolYearSpecification(ids: query.SchoolYearIds), ct);
            return schoolYears
                .Select(schoolYear => new SchoolYearResponse(
                    schoolYear.Id,
                    schoolYear.SchoolId,
                    schoolYear.Label,
                    schoolYear.StartDate,
                    schoolYear.EndDate,
                    schoolYear.Status.ToString()))
                .ToList();
        }
    }
}
