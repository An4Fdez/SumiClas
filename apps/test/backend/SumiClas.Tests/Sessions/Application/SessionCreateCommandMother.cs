namespace SumiClas.Tests.Sessions.Application;

using SumiClas.Sessions.Application;

public static class SessionCreateCommandMother
{
    public static SessionCreate.Command WithCredentials(string email, string password)
        => new(email, password);
}
