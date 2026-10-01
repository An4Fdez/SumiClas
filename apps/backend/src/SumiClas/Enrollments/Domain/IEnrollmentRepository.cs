using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public interface IEnrollmentRepository
{
    Task<IReadOnlyCollection<Enrollment>> SearchAsync(ISpecification<Enrollment> specification, CancellationToken ct = default);
    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
}
