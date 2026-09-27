namespace SumiClas.Shared.Domain;

public abstract class Entity
{
    public Guid Id { get; protected init; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? a, Entity? b) => Equals(a, b);
    public static bool operator !=(Entity? a, Entity? b) => !Equals(a, b);
}
