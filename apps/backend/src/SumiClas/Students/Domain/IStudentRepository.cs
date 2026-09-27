namespace SumiClas.Students.Domain;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Student>> GetByGuardianIdAsync(Guid guardianId, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
}
