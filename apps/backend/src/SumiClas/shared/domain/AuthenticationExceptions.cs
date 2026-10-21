namespace SumiClas.Shared.Domain;

public static class AuthenticationExceptions
{
    public sealed class Unauthenticated()
        : DomainException(AuthenticationErrors.Unauthenticated.Code, AuthenticationErrors.Unauthenticated.Message);
}
