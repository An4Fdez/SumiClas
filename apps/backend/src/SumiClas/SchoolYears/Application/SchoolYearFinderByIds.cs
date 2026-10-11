using MediatR;
using SumiClas.Shared.Application;
using SumiClas.SchoolYears.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearFinderByIds
{
    /// <summary>Returns only the school years that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> SchoolYearIds) : IQuery<IReadOnlyCollection<SchoolYearResponse>>;

    public sealed class Finder(ISchoolYearRepository schoolYearRepository) : IUseCase
    {
        public async Task<IReadOnlyCollection<SchoolYearResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
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

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<SchoolYearResponse>>
    {
        public Task<IReadOnlyCollection<SchoolYearResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
