using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain.Services;

public sealed class SchoolYearFinder(ISchoolYearRepository schoolYears)
{
    public async Task<Result<SchoolYear>> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var schoolYear = await schoolYears.GetByIdAsync(id, ct);
        return schoolYear is null
            ? Result<SchoolYear>.Failure(SchoolYearErrors.NotFound)
            : Result<SchoolYear>.Success(schoolYear);
    }
}
