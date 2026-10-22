using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>
/// The HTTP status of a domain exception when its endpoint does not override it with
/// <see cref="DomainExceptionEndpointExtensions.Maps{TException}"/>, decided by the suffix of the exception name:
/// <c>NotFound</c> is 404; <c>Forbidden</c> is 403; <c>AlreadyExists</c>, <c>AlreadyConfirmed</c>,
/// <c>AlreadyRegistered</c> and <c>ConcurrencyConflict</c> are 409; <c>Unauthenticated</c>, <c>InvalidCredentials</c>
/// and <c>RefreshTokenInvalid</c> are 401; every other exception is a validation failure, 400.
/// The domain knows nothing of HTTP: the rule lives here, in the API layer, and a test pins the exceptions that
/// fall in the default so a misspelled name cannot silently become a 400.
/// </summary>
public static class DomainExceptionConvention
{
    private static readonly (string[] Suffixes, int StatusCode)[] Rules =
    [
        (["NotFound"], StatusCodes.Status404NotFound),
        (["Forbidden"], StatusCodes.Status403Forbidden),
        (["AlreadyExists", "AlreadyConfirmed", "AlreadyRegistered", "ConcurrencyConflict"], StatusCodes.Status409Conflict),
        (["Unauthenticated", "InvalidCredentials", "RefreshTokenInvalid"], StatusCodes.Status401Unauthorized),
    ];

    public static int StatusOf(Type exceptionType)
    {
        foreach ((string[] suffixes, int statusCode) in Rules)
        {
            if (suffixes.Any(suffix => exceptionType.Name.EndsWith(suffix, StringComparison.Ordinal)))
            {
                return statusCode;
            }
        }

        return StatusCodes.Status400BadRequest;
    }

    public static int StatusOf(DomainException exception) => StatusOf(exception.GetType());
}
