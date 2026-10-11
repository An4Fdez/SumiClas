namespace SumiClas.Sessions.Application;

/// <summary>
/// Closed result of a refresh. Modeled as a successful outcome instead of a failure because a failed result rolls the
/// transaction back and the revocation of a reused session has to persist.
/// </summary>
public abstract record SessionRotateOutcome
{
    private SessionRotateOutcome() { }

    public sealed record Rotated(SessionResponse Session) : SessionRotateOutcome;

    public sealed record Invalid : SessionRotateOutcome;

    public sealed record Reused : SessionRotateOutcome;
}
