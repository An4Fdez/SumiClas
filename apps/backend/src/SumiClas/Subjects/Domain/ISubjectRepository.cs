namespace SumiClas.Subjects.Domain;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Subject subject, CancellationToken ct = default);
}
