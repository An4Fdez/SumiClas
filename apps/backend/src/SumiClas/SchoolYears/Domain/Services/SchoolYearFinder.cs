namespace SumiClas.SchoolYears.Domain.Services;

public sealed class SchoolYearFinder(ISchoolYearRepository schoolYears)
{
    public async Task<SchoolYear> FindByIdAsync(Guid id, CancellationToken ct = default)
        => await schoolYears.GetByIdAsync(id, ct) ?? throw new SchoolYearExceptions.NotFound();
}
