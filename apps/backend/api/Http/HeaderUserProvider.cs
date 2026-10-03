using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>
/// PROVISIONAL: takes the current user from the X-User-Id header, which the client chooses freely, so it proves nothing.
/// Replace it with the signed-in user (JWT claim) once authentication is decided; use cases only see IUserProvider.
/// </summary>
public sealed class HeaderUserProvider(IHttpContextAccessor httpContextAccessor) : IUserProvider
{
    public const string HeaderName = "X-User-Id";

    public Result<Guid> GetCurrentUserId()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();

        return Guid.TryParse(header, out var userId) && userId != Guid.Empty
            ? Result<Guid>.Success(userId)
            : Result<Guid>.Failure(AuthenticationErrors.Unauthenticated);
    }
}
