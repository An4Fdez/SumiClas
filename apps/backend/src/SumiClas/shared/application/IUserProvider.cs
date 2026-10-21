using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// The user on whose behalf the current request runs ("who is doing this"), so commands never carry an
/// author/creator/acting user id. The implementation lives in the api layer: <c>JwtUserProvider</c> reads the
/// <c>sub</c> claim of the validated access token.
/// </summary>
public interface IUserProvider
{
    /// <summary>The current user id; throws <see cref="AuthenticationExceptions.Unauthenticated"/> when nobody is identified.</summary>
    Guid GetCurrentUserId();
}
