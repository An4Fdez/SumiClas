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

    public SchoolYear(
        Guid id,
        Guid schoolId,
        string label,
        DateOnly startDate,
        DateOnly endDate,
        SchoolYearStatus status) : base(id)
    {
        SchoolId = schoolId;
        Label = label;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    public static SchoolYear Create(Guid id, Guid schoolId, string label, DateOnly startDate, DateOnly endDate)
    {
        Validate(id, schoolId, label, startDate, endDate);

        return new SchoolYear(id, schoolId, label.Trim(), startDate, endDate, SchoolYearStatus.Active);
    }

    private static void Validate(Guid id, Guid schoolId, string label, DateOnly startDate, DateOnly endDate)
    {
        if (id == Guid.Empty)
        {
            throw new SchoolYearExceptions.IdRequired();
        }
        if (schoolId == Guid.Empty)
        {
            throw new SchoolYearExceptions.SchoolRequired();
        }
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new SchoolYearExceptions.LabelRequired();
        }
        if (endDate <= startDate)
        {
            throw new SchoolYearExceptions.InvalidDateRange();
        }
    }

    /// <summary>
    /// Guard used by any future module (via the MediatR QueryBus) before writing
    /// to anything hanging off a Class that belongs to this school year.
    /// </summary>
    public void EnsureIsActive()
    {
        if (IsArchived)
        {
            throw new SchoolYearExceptions.Archived();
        }
    }

    public void Archive(IClock clock)
    {
        EnsureIsActive();

        Status = SchoolYearStatus.Archived;
        AddDomainEvent(new SchoolYearArchivedDomainEvent(Id, clock));
    }
}
