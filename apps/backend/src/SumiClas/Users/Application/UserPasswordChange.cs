using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserPasswordChange
{
    /// <summary>
    /// The user is already authenticated, so a wrong current password has its own 400 instead of the 401 of a failed
    /// login (a 401 on an open session would make the client believe the session expired).
    /// </summary>
    public sealed record Command(string CurrentPassword, string NewPassword) : IRequest, ICommand;

    /// <summary>
    /// The new password is checked against the policy before the current one is verified, so nothing is hashed for a
    /// request that cannot succeed. Closing every session of the user is a reaction to the event the user raises
    /// (see <c>SessionRevokeAllOnUserPasswordChange</c>), in the same transaction.
    /// </summary>
    public sealed class Change(
        IUserRepository repository,
        IPasswordHasher hasher,
        IUserProvider userProvider,
        IClock clock) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid userId = userProvider.GetCurrentUserId();

            User user = await repository.GetByIdAsync(userId, ct)
                ?? throw new UserExceptions.NotFound();

            Password newPassword = Password.Create(command.NewPassword);

            if (hasher.Verify(user.PasswordHash, command.CurrentPassword ?? string.Empty) == PasswordVerification.Failed)
            {
                throw new UserExceptions.CurrentPasswordInvalid();
            }

            user.ChangePassword(hasher.Generate(newPassword), clock);
        }
    }

    public sealed class Handler(Change change) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => change.ExecuteAsync(command, ct);
    }
}
