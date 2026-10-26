using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Assignment>> SearchAsync(ISpecification<Assignment> specification, CancellationToken ct = default);
    Task AddAsync(Assignment assignment, CancellationToken ct = default);
    void Remove(Assignment assignment);
}
