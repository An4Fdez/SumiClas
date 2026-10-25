namespace SumiClas.Schools.Domain.Services;

public sealed class SchoolFinder(ISchoolRepository schools)
{
    public async Task<School> FindAsync(Guid id, CancellationToken ct = default)
        => await schools.GetByIdAsync(id, ct) ?? throw new SchoolExceptions.NotFound();
}
