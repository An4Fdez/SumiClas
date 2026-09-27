namespace SumiClas.Shared.Domain;

public abstract class Entity
{
    public Guid Id { get; protected init; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>EF Core materialization only, for aggregates whose real constructor takes a parameter EF cannot bind (e.g. IClock).</summary>
    protected Entity()
    {
    }

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? a, Entity? b) => Equals(a, b);
    public static bool operator !=(Entity? a, Entity? b) => !Equals(a, b);
}
