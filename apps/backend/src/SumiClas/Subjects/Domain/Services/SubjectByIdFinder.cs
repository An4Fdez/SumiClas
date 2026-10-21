namespace SumiClas.Subjects.Domain.Services;

public sealed class SubjectByIdFinder(ISubjectRepository subjects)
{
    public async Task<Subject> FindAsync(Guid id, CancellationToken ct = default)
        => await subjects.GetByIdAsync(id, ct) ?? throw new SubjectExceptions.NotFound();
}
