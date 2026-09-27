namespace SumiClas.Shared.Domain;

public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEventBase> _domainEvents = new();
    public IReadOnlyCollection<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>EF Core materialization only — see Entity's parameterless constructor.</summary>
    protected AggregateRoot()
    {
    }

    protected void AddDomainEvent(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
