namespace SumiClas.Shared.Domain.Events;

/// <summary>
/// Raised by User.ChangePassword. Lives in shared because Sessions reacts to it (closing every session of the user)
/// and modules must never reference each other's Domain.
/// </summary>
public sealed class UserPasswordChangedDomainEvent : DomainEventBase
{
    public Guid UserId { get; }

    public UserPasswordChangedDomainEvent(Guid userId, IClock clock) : base(clock)
    {
        UserId = userId;
    }
}
