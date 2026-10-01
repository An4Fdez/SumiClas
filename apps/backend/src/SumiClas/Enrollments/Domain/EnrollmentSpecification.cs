using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public sealed class EnrollmentSpecification : Specification<Enrollment>
{
    public EnrollmentSpecification(
        Guid? id = null,
        Guid? studentId = null,
        Guid? classId = null,
        EnrollmentStatus? status = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? confirmedAt = null)
    {
        if (id is { } enrollmentId) AddCriteria(enrollment => enrollment.Id == enrollmentId);
        if (studentId is { } student) AddCriteria(enrollment => enrollment.StudentId == student);
        if (classId is { } schoolClass) AddCriteria(enrollment => enrollment.ClassId == schoolClass);
        if (status is { } state) AddCriteria(enrollment => enrollment.Status == state);
        if (createdAt is { } created) AddCriteria(enrollment => enrollment.CreatedAt == created);
        if (confirmedAt is { } confirmed) AddCriteria(enrollment => enrollment.ConfirmedAt == confirmed);
    }
}
