namespace SumiClas.Subjects.Domain;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Subject>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task AddAsync(Subject subject, CancellationToken ct = default);
}
