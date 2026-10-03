namespace SumiClas.Assignments.Domain;

public interface IAssignmentRepository
{
    Task AddAsync(Assignment assignment, CancellationToken ct = default);
}
