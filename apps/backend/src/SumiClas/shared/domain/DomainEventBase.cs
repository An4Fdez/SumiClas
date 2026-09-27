namespace SumiClas.Shared.Domain;

public abstract class DomainEventBase
{
    public DateTimeOffset OccurredOn { get; }

    protected DomainEventBase(IClock clock)
    {
        OccurredOn = clock.UtcNow;
    }
}
