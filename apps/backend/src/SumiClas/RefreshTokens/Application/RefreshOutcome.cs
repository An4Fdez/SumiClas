namespace SumiClas.RefreshTokens.Application;

/// <summary>
/// Closed result of a refresh. Modeled as a successful outcome instead of a failure because a failed result rolls the
/// transaction back and the revocation of a reused family has to persist.
/// </summary>
public abstract record RefreshOutcome
{
    private RefreshOutcome() { }

    public sealed record Rotated(SessionResponse Session) : RefreshOutcome;

    public sealed record Invalid : RefreshOutcome;

    public sealed record Reused : RefreshOutcome;
}
