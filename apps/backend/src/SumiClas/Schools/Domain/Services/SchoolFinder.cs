using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain.Services;

public sealed class SchoolFinder(ISchoolRepository schools)
{
    public async Task<Result<School>> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var school = await schools.GetByIdAsync(id, ct);
        return school is null
            ? Result<School>.Failure(SchoolErrors.NotFound)
            : Result<School>.Success(school);
    }
}
