using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Domain.Services;

public sealed class ClassByIdFinder(IClassRepository classes)
{
    public async Task<Result<Class>> FindAsync(Guid id, CancellationToken ct = default)
    {
        var schoolClass = await classes.GetByIdAsync(id, ct);
        return schoolClass is null
            ? Result<Class>.Failure(ClassErrors.NotFound)
            : Result<Class>.Success(schoolClass);
    }
}
