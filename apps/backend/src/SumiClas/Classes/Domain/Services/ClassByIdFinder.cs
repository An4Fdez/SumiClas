namespace SumiClas.Classes.Domain.Services;

public sealed class ClassByIdFinder(IClassRepository classes)
{
    public async Task<Class> FindAsync(Guid id, CancellationToken ct = default)
        => await classes.GetByIdAsync(id, ct) ?? throw new ClassExceptions.NotFound();
}
