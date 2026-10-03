using SumiClas.Shared.Domain;

namespace SumiClas.Subjects.Domain.Services;

public sealed class SubjectByIdFinder(ISubjectRepository subjects)
{
    public async Task<Result<Subject>> FindAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await subjects.GetByIdAsync(id, ct);
        return subject is null
            ? Result<Subject>.Failure(SubjectErrors.NotFound)
            : Result<Subject>.Success(subject);
    }
}
