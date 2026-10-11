using MediatR;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearFinderBySchool
{
    /// <summary>The school years of a school, newest first; <paramref name="Status"/> ("Active"/"Archived") is optional.</summary>
    public sealed record Query(Guid SchoolId, string? Status) : IQuery<Result<IReadOnlyCollection<SchoolYearResponse>>>;

    public sealed class Finder(ISchoolYearRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<Result<IReadOnlyCollection<SchoolYearResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            SchoolYearStatus? status = null;
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (!Enum.TryParse<SchoolYearStatus>(query.Status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
                {
                    return Result<IReadOnlyCollection<SchoolYearResponse>>.Failure(SchoolYearErrors.InvalidStatus);
                }

                status = parsed;
            }

            var school = await queryBus.Send(new SchoolFinderById.Query(query.SchoolId), ct);
            if (school.IsFailure)
            {
                return Result<IReadOnlyCollection<SchoolYearResponse>>.Failure(school.Error!);
            }

            var schoolYears = await repository.SearchAsync(new SchoolYearSpecification(schoolId: query.SchoolId, status: status), ct);
            IReadOnlyCollection<SchoolYearResponse> response = schoolYears
                .OrderByDescending(schoolYear => schoolYear.StartDate)
                .Select(schoolYear => new SchoolYearResponse(
                    schoolYear.Id,
                    schoolYear.SchoolId,
                    schoolYear.Label,
                    schoolYear.StartDate,
                    schoolYear.EndDate,
                    schoolYear.Status.ToString()))
                .ToList();

            return Result<IReadOnlyCollection<SchoolYearResponse>>.Success(response);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<IReadOnlyCollection<SchoolYearResponse>>>
    {
        public Task<Result<IReadOnlyCollection<SchoolYearResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
