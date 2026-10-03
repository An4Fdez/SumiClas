namespace SumiClas.Shared.Domain.Events;

/// <summary>
/// Raised by Class.Create. Carries the data of the aggregate plus the creator, which is not a field of Class
/// but is what other modules need to react (the creator becomes the first delegate).
/// Lives in shared because it is consumed across modules, which must never reference each other's Domain.
/// </summary>
public sealed class ClassCreatedDomainEvent : DomainEventBase
{
    public Guid ClassId { get; }
    public Guid SchoolYearId { get; }
    public string Name { get; }
    public Guid CreatorId { get; }

    public ClassCreatedDomainEvent(Guid classId, Guid schoolYearId, string name, Guid creatorId, IClock clock) : base(clock)
    {
        ClassId = classId;
        SchoolYearId = schoolYearId;
        Name = name;
        CreatorId = creatorId;
    }
}
