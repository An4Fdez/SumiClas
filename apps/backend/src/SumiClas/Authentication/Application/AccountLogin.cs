using MediatR;
using SumiClas.Authentication.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Authentication.Application;

/// <summary>
/// Checks an email and a password. An unknown email, an account without credential and a wrong password all fail
/// with the same <see cref="AuthenticationErrors.InvalidCredentials"/> (and all pay for one hash verification), so the
/// answer does not reveal which accounts exist. A command, not a query: it may store a stronger hash.
/// </summary>
public static class AccountLogin
{
    public sealed record Command(string Email, string Password) : IRequest<Result<AccountResponse>>, ICommand;

    public sealed class UseCase(IQueryBus queryBus, IUserCredentialRepository credentials, IPasswordHasher hasher, IClock clock)
    {
        public async Task<Result<AccountResponse>> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var password = command.Password ?? string.Empty;

            var user = await queryBus.Send(new UserFinderByEmail.Query(command.Email ?? string.Empty), ct);
            var credential = user.IsSuccess ? await credentials.GetByIdAsync(user.Value.Id, ct) : null;

            var verification = hasher.Verify(credential?.PasswordHash, password);
            if (user.IsFailure || credential is null || verification == PasswordVerification.Failed)
            {
                return Result<AccountResponse>.Failure(AuthenticationErrors.InvalidCredentials);
            }

            if (verification == PasswordVerification.SuccessRehashNeeded)
            {
                var rehash = credential.ReplacePasswordHash(hasher.Hash(password), clock);
                if (rehash.IsFailure)
                {
                    return Result<AccountResponse>.Failure(rehash.Error!);
                }
            }

            return Result<AccountResponse>.Success(new AccountResponse(user.Value.Id, user.Value.FullName));
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result<AccountResponse>>
    {
        public Task<Result<AccountResponse>> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
