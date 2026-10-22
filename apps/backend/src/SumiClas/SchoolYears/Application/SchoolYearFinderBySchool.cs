using MediatR;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearFinderBySchool
{
    /// <summary>The school years of a school, newest first; <paramref name="Status"/> ("Active"/"Archived") is optional.</summary>
    public sealed record Query(Guid SchoolId, string? Status) : IQuery<IReadOnlyCollection<SchoolYearResponse>>;

    public sealed class Finder(ISchoolYearRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<IReadOnlyCollection<SchoolYearResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            SchoolYearStatus? status = null;
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (!Enum.TryParse<SchoolYearStatus>(query.Status, ignoreCase: true, out SchoolYearStatus parsed) || !Enum.IsDefined(parsed))
                {
                    throw new SchoolYearExceptions.InvalidStatus();
                }

                status = parsed;
            }

            await queryBus.Send(new SchoolEnsure.Query(query.SchoolId), ct);

            IReadOnlyCollection<SchoolYear> schoolYears = await repository.SearchAsync(new SchoolYearSpecification(schoolId: query.SchoolId, status: status), ct);
            return schoolYears
                .OrderByDescending(schoolYear => schoolYear.StartDate)
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
