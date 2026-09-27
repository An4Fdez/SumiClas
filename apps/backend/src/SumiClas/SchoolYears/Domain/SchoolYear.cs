using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain;

public sealed class SchoolYear : AggregateRoot
{
    public Guid SchoolId { get; private set; }
    public string Label { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public SchoolYearStatus Status { get; private set; }

    public bool IsArchived => Status == SchoolYearStatus.Archived;

    private SchoolYear(Guid id, Guid schoolId, string label, DateOnly startDate, DateOnly endDate) : base(id)
    {
        SchoolId = schoolId;
        Label = label;
        StartDate = startDate;
        EndDate = endDate;
        Status = SchoolYearStatus.Active;
    }

    public static Result<SchoolYear> Create(Guid schoolId, string label, DateOnly startDate, DateOnly endDate)
    {
        var validation = Validate(schoolId, label, startDate, endDate);
        if (validation.IsFailure)
        {
            return Result<SchoolYear>.Failure(validation.Error!);
        }

        var schoolYear = new SchoolYear(Guid.NewGuid(), schoolId, label.Trim(), startDate, endDate);
        return Result<SchoolYear>.Success(schoolYear);
    }

    private static Result Validate(Guid schoolId, string label, DateOnly startDate, DateOnly endDate)
    {
        if (schoolId == Guid.Empty)
        {
            return Result.Failure(SchoolYearErrors.SchoolRequired);
        }
        if (string.IsNullOrWhiteSpace(label))
        {
            return Result.Failure(SchoolYearErrors.LabelRequired);
        }
        if (endDate <= startDate)
        {
            return Result.Failure(SchoolYearErrors.InvalidDateRange);
        }
        return Result.Success();
    }

    /// <summary>
    /// Guard used by any future module (via the MediatR QueryBus) before writing
    /// to anything hanging off a Class that belongs to this school year.
    /// </summary>
    public Result EnsureIsActive()
    {
        return IsArchived ? Result.Failure(SchoolYearErrors.Archived) : Result.Success();
    }

    public Result Archive(IClock clock)
    {
        var activeCheck = EnsureIsActive();
        if (activeCheck.IsFailure)
        {
            return activeCheck;
        }

        Status = SchoolYearStatus.Archived;
        AddDomainEvent(new SchoolYearArchivedDomainEvent(Id, clock));
        return Result.Success();
    }
}
