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
    public static Result<Enrollment> Create(Guid id, Guid studentId, Guid classId, IClock clock)
    {
        var validation = Validate(id, studentId, classId);
        if (validation.IsFailure)
        {
            return Result<Enrollment>.Failure(validation.Error!);
        }

        var now = clock.UtcNow;
        var enrollment = new Enrollment(id, studentId, classId, EnrollmentStatus.Confirmed, now, now);
        return Result<Enrollment>.Success(enrollment);
    }

    /// <summary>Suggested continuity when a new SchoolYear opens: pending guardian confirmation.</summary>
    public static Result<Enrollment> CreateSuggested(Guid id, Guid studentId, Guid classId, IClock clock)
    {
        var validation = Validate(id, studentId, classId);
        if (validation.IsFailure)
        {
            return Result<Enrollment>.Failure(validation.Error!);
        }

        var enrollment = new Enrollment(id, studentId, classId, EnrollmentStatus.Suggested, clock.UtcNow, null);
        return Result<Enrollment>.Success(enrollment);
    }

    public Result Confirm(IClock clock)
    {
        if (Status == EnrollmentStatus.Confirmed)
        {
            return Result.Failure(EnrollmentErrors.AlreadyConfirmed);
        }

        Status = EnrollmentStatus.Confirmed;
        ConfirmedAt = clock.UtcNow;
        return Result.Success();
    }

    private static Result Validate(Guid id, Guid studentId, Guid classId)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(EnrollmentErrors.IdRequired);
        }
        if (studentId == Guid.Empty)
        {
            return Result.Failure(EnrollmentErrors.StudentRequired);
        }
        if (classId == Guid.Empty)
        {
            return Result.Failure(EnrollmentErrors.ClassRequired);
        }
        return Result.Success();
    }
}
