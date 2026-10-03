using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Student>> SearchAsync(ISpecification<Student> specification, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
}
