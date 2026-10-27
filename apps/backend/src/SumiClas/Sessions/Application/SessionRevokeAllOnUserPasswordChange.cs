using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;

namespace SumiClas.Sessions.Application;

/// <summary>
/// A new password closes every session of the user, the one that changed it included (the refresh cookie never
/// reaches that endpoint, so the current session cannot be told apart). Runs in the same transaction as the change.
/// An access token already issued stays valid until it expires (<see cref="AccessToken.Lifetime"/>).
/// </summary>
public sealed class SessionRevokeAllOnUserPasswordChange(ISessionRepository repository, IClock clock)
    : IDomainEventHandler<UserPasswordChangedDomainEvent>
{
    public async Task Handle(UserPasswordChangedDomainEvent domainEvent, CancellationToken ct = default)
    {
        IReadOnlyCollection<Session> sessions = await repository.SearchAsync(
            new SessionSpecification(userId: domainEvent.UserId), ct);

        foreach (Session session in sessions)
        {
            session.Revoke(clock);
        }
    }
}
