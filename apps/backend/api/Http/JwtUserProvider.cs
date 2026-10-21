using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>Takes the current user id from the <c>sub</c> (subject) claim of the access token the JWT bearer middleware already validated.</summary>
public sealed class JwtUserProvider(IHttpContextAccessor httpContextAccessor) : IUserProvider
{
    private const string UserIdClaim = "sub";

    public Guid GetCurrentUserId()
    {
        var userIdHttp = httpContextAccessor.HttpContext?.User.FindFirst(UserIdClaim)?.Value;

        return Guid.TryParse(userIdHttp, out var userId) && userId != Guid.Empty
            ? userId
            : throw new AuthenticationExceptions.Unauthenticated();
    }
}
