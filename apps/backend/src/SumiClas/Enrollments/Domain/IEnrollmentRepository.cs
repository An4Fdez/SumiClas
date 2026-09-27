namespace SumiClas.Enrollments.Domain;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Enrollment>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Enrollment>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);

    /// <summary>
    /// Supports the cross-instance invariant "one confirmed enrollment per student per school year",
    /// which no single Enrollment aggregate can protect on its own — the future Application layer
    /// must check this before confirming a new enrollment.
    /// </summary>
    Task<bool> HasConfirmedEnrollmentForSchoolYearAsync(Guid studentId, Guid schoolYearId, CancellationToken ct = default);

    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
}
