using System.Linq.Expressions;
using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public sealed class ConfirmedEnrollmentsByStudentInClassesSpecification(Guid studentId, IReadOnlyCollection<Guid> classIds)
    : ISpecification<Enrollment>
{
    private readonly Guid[] _classIds = classIds.ToArray();

    public Expression<Func<Enrollment, bool>> Criteria
        => enrollment => enrollment.StudentId == studentId
            && enrollment.Status == EnrollmentStatus.Confirmed
            && _classIds.Contains(enrollment.ClassId);
}
