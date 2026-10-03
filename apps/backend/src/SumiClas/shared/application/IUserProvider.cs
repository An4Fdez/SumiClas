using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// The user on whose behalf the current request runs ("who is doing this"), so commands never carry an
/// author/creator/acting user id. The implementation lives in the api layer: today it reads the X-User-Id header
/// (provisional, anyone can claim any id); when authentication exists it will read the signed-in user instead.
/// </summary>
public interface IUserProvider
{
    /// <summary>The current user id, or <see cref="AuthenticationErrors.Unauthenticated"/> when nobody is identified.</summary>
    Result<Guid> GetCurrentUserId();
}
