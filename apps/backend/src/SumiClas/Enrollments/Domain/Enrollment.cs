using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public sealed class Enrollment : AggregateRoot
{
    public Guid StudentId { get; private set; }
    public Guid ClassId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }

    public Enrollment(
        Guid id,
        Guid studentId,
        Guid classId,
        EnrollmentStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? confirmedAt) : base(id)
    {
        StudentId = studentId;
        ClassId = classId;
        Status = status;
        CreatedAt = createdAt;
        ConfirmedAt = confirmedAt;
    }

    /// <summary>EF Core materialization only — the real constructor takes an IClock, which EF cannot bind.</summary>
    private Enrollment()
    {
    }

    /// <summary>Direct enrollment: a guardian enrolls their child in a class, already confirmed.</summary>
    public static Enrollment Create(Guid id, Guid studentId, Guid classId, IClock clock)
    {
        Validate(id, studentId, classId);

        var now = clock.UtcNow;
        return new Enrollment(id, studentId, classId, EnrollmentStatus.Confirmed, now, now);
    }

    /// <summary>Suggested continuity when a new SchoolYear opens: pending guardian confirmation.</summary>
    public static Enrollment CreateSuggested(Guid id, Guid studentId, Guid classId, IClock clock)
    {
        Validate(id, studentId, classId);

        return new Enrollment(id, studentId, classId, EnrollmentStatus.Suggested, clock.UtcNow, null);
    }

    public void Confirm(IClock clock)
    {
        if (Status == EnrollmentStatus.Confirmed)
        {
            throw new EnrollmentExceptions.AlreadyConfirmed();
        }

        Status = EnrollmentStatus.Confirmed;
        ConfirmedAt = clock.UtcNow;
    }

    private static void Validate(Guid id, Guid studentId, Guid classId)
    {
        if (id == Guid.Empty)
        {
            throw new EnrollmentExceptions.IdRequired();
        }
        if (studentId == Guid.Empty)
        {
            throw new EnrollmentExceptions.StudentRequired();
        }
        if (classId == Guid.Empty)
        {
            throw new EnrollmentExceptions.ClassRequired();
        }
    }
}
