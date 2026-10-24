using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>The standard ProblemDetails response for a domain exception: its code is the title and the <c>code</c> extension.</summary>
public static class DomainProblem
{
    public static IResult Create(DomainException exception, int statusCode)
        => ApiProblem.Create(exception.Code, exception.Message, statusCode);

    public static IResult Unauthenticated()
        => Create(new AuthenticationExceptions.Unauthenticated(), StatusCodes.Status401Unauthorized);

    public static IResult Forbidden()
        => Create(new AuthenticationExceptions.Forbidden(), StatusCodes.Status403Forbidden);

    public static IResult RefreshTokenInvalid()
        => Create(new AuthenticationExceptions.RefreshTokenInvalid(), StatusCodes.Status401Unauthorized);
}
