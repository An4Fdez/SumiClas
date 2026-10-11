using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserPasswordVerify
{
    /// <summary>
    /// An unknown email and a wrong password all fail with the same
    /// <see cref="AuthenticationErrors.InvalidCredentials"/> (and all pay for one hash verification), so the answer does
    /// not reveal which accounts exist. It may replace the stored hash with a stronger one: the user is tracked
    /// and the root command's unit of work saves it.
    /// </summary>
    public sealed record Query(string Email, string Password) : IQuery<Result<UserResponse>>;

    public sealed class Verify(IUserRepository repository, IPasswordHasher hasher, IClock clock) : IUseCase
    {
        public async Task<Result<UserResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var password = query.Password ?? string.Empty;

            var email = Email.Create(query.Email ?? string.Empty);
            var user = email.IsSuccess
                ? (await repository.SearchAsync(new UserSpecification(email: email.Value), ct)).FirstOrDefault()
                : null;

            var verification = hasher.Verify(user?.PasswordHash, password);
            if (user is null || verification == PasswordVerification.Failed)
            {
                return Result<UserResponse>.Failure(AuthenticationErrors.InvalidCredentials);
            }

            if (verification == PasswordVerification.SuccessRehashNeeded)
            {
                // A legacy password outside the current rules keeps its old hash: login must still succeed.
                var rehashable = Password.Create(password);
                if (rehashable.IsSuccess)
                {
                    var rehash = user.ReplacePasswordHash(hasher.Generate(rehashable.Value), clock);
                    if (rehash.IsFailure)
                    {
                        return Result<UserResponse>.Failure(rehash.Error!);
                    }
                }
            }

            return Result<UserResponse>.Success(new UserResponse(user.Id, user.FullName));
        }
    }

    public sealed class Handler(Verify verify) : IRequestHandler<Query, Result<UserResponse>>
    {
        public Task<Result<UserResponse>> Handle(Query query, CancellationToken ct)
            => verify.ExecuteAsync(query, ct);
    }
}
