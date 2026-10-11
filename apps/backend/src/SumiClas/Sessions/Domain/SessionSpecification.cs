using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public sealed class SessionSpecification : Specification<Session>
{
    /// <param name="tokenHash">Matches sessions that have a refresh token with this hash.</param>
    public SessionSpecification(Guid? id = null, Guid? userId = null, string? tokenHash = null)
    {
        if (id is { } sessionId) AddCriteria(session => session.Id == sessionId);
        if (userId is { } user) AddCriteria(session => session.UserId == user);
        if (tokenHash is not null) AddCriteria(session => session.RefreshTokens.Any(token => token.TokenHash == tokenHash));
    }
}
