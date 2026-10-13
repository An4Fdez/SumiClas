namespace SumiClas.Tests.Sessions.Application;

using SumiClas.Sessions.Application;

public static class SessionRotateOutcomeMother
{
    public static SessionRotateOutcome.Rotated Rotated(SessionResponse session)
        => new(session);
}
