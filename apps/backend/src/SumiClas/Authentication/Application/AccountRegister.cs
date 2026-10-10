using MediatR;
using SumiClas.Authentication.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Application;

/// <summary>
/// Creates the credential of a new account. The profile (<c>User</c>) is created by the Users module when it handles
/// the event the credential raises, inside the same transaction.
/// </summary>
public static class AccountRegister
{
    public sealed record Command(Guid Id, string FullName, string Email, string Password) : IRequest<Result>, ICommand;

    public sealed class UseCase(IUserCredentialRepository credentials, IPasswordHasher hasher, IClock clock)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var policy = PasswordPolicy.Validate(command.Password);
            if (policy.IsFailure)
            {
                return policy;
            }

            var creation = UserCredential.Create(
                command.Id, hasher.Hash(command.Password), command.FullName, command.Email, clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            await credentials.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
