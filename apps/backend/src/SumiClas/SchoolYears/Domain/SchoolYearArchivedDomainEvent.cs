using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain;

public sealed class SchoolYearArchivedDomainEvent : DomainEventBase
{
    public Guid SchoolYearId { get; }

    public SchoolYearArchivedDomainEvent(Guid schoolYearId, IClock clock) : base(clock)
    {
        SchoolYearId = schoolYearId;
    }
}
