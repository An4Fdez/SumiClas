using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public sealed class Enrollment : AggregateRoot
{
    public Guid StudentId { get; private set; }
    public Guid ClassId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }

    private Enrollment(Guid id, Guid studentId, Guid classId, EnrollmentStatus status, IClock clock) : base(id)
    {
        StudentId = studentId;
        ClassId = classId;
        Status = status;
        CreatedAt = clock.UtcNow;
    }

    /// <summary>Direct enrollment: a guardian enrolls their child in a class, already confirmed.</summary>
    public static Result<Enrollment> Create(Guid studentId, Guid classId, IClock clock)
    {
        var validation = Validate(studentId, classId);
        if (validation.IsFailure)
        {
            return Result<Enrollment>.Failure(validation.Error!);
        }

        var enrollment = new Enrollment(Guid.NewGuid(), studentId, classId, EnrollmentStatus.Confirmed, clock)
        {
            ConfirmedAt = clock.UtcNow,
        };
        return Result<Enrollment>.Success(enrollment);
    }

    /// <summary>Suggested continuity when a new SchoolYear opens: pending guardian confirmation.</summary>
    public static Result<Enrollment> CreateSuggested(Guid studentId, Guid classId, IClock clock)
    {
        var validation = Validate(studentId, classId);
        if (validation.IsFailure)
        {
            return Result<Enrollment>.Failure(validation.Error!);
        }

        var enrollment = new Enrollment(Guid.NewGuid(), studentId, classId, EnrollmentStatus.Suggested, clock);
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

    private static Result Validate(Guid studentId, Guid classId)
    {
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
