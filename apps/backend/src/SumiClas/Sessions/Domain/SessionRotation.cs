namespace SumiClas.Sessions.Domain;

/// <summary>Closed result of <see cref="Session.Rotate"/>: a rotation that does not apply is an outcome, not an error.</summary>
public abstract record SessionRotation
{
    private SessionRotation() { }

    public sealed record Rotated(RefreshToken NewToken) : SessionRotation;

    public sealed record Invalid : SessionRotation;

    public sealed record Reused : SessionRotation;
}
