namespace SumiClas.Shared.Domain.Events;

/// <summary>
/// Raised by UserCredential.Create. Carries what the Users module needs to create the profile of the new account.
/// It never carries the password or its hash. Lives in shared because it is consumed across modules, which must
/// never reference each other's Domain.
/// </summary>
public sealed class UserCredentialCreatedDomainEvent : DomainEventBase
{
    public Guid UserId { get; }
    public string FullName { get; }
    public string Email { get; }

    public UserCredentialCreatedDomainEvent(Guid userId, string fullName, string email, IClock clock) : base(clock)
    {
        UserId = userId;
        FullName = fullName;
        Email = email;
    }
}
