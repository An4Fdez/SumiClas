using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

/// <summary>
/// Creates the profile of a new account when its credential is created. Runs in the same transaction as the
/// registration: if the profile cannot be created (invalid name or email, email already taken) the handler fails and
/// the credential is not saved either. A race on the unique email index is still caught as a 409 by the unit of work.
/// </summary>
public sealed class UserRegisterOnUserCredentialCreated(IUserRepository users) : IDomainEventHandler<UserCredentialCreatedDomainEvent>
{
    public async Task<Result> Handle(UserCredentialCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var email = Email.Create(domainEvent.Email);
        if (email.IsFailure)
        {
            return Result.Failure(email.Error!);
        }

        var registration = User.Register(domainEvent.UserId, domainEvent.FullName, email.Value);
        if (registration.IsFailure)
        {
            return Result.Failure(registration.Error!);
        }

        var existing = await users.SearchAsync(new UserSpecification(email: email.Value), ct);
        if (existing.Count > 0)
        {
            return Result.Failure(UserErrors.EmailAlreadyRegistered);
        }

        await users.AddAsync(registration.Value, ct);

        return Result.Success();
    }
}
