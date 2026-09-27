namespace SumiClas.Assignments.Domain;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Assignment>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task AddAsync(Assignment assignment, CancellationToken ct = default);
}
