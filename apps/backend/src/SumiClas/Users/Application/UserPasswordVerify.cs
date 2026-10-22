using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserPasswordVerify
{
    /// <summary>
    /// An unknown email and a wrong password all fail with the same
    /// <see cref="AuthenticationExceptions.InvalidCredentials"/> (and all pay for one hash verification), so the answer
    /// does not reveal which accounts exist. It may replace the stored hash with a stronger one: the user is tracked
    /// and the root command's unit of work saves it.
    /// </summary>
    public sealed record Query(string Email, string Password) : IQuery<UserResponse>;

    public sealed class Verify(IUserRepository repository, IPasswordHasher hasher, IClock clock) : IUseCase
    {
        public async Task<UserResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var password = query.Password ?? string.Empty;

            var user = Email.TryCreate(query.Email, out var email)
                ? (await repository.SearchAsync(new UserSpecification(email: email), ct)).FirstOrDefault()
                : null;

            var verification = hasher.Verify(user?.PasswordHash, password);
            if (user is null || verification == PasswordVerification.Failed)
            {
                throw new AuthenticationExceptions.InvalidCredentials();
            }

            // A legacy password outside the current rules keeps its old hash: login must still succeed.
            if (verification == PasswordVerification.SuccessRehashNeeded && Password.TryCreate(password, out var rehashable))
            {
                user.ReplacePasswordHash(hasher.Generate(rehashable), clock);
            }

            return new UserResponse(user.Id, user.FullName);
        }
    }

    public sealed class Handler(Verify verify) : IRequestHandler<Query, UserResponse>
    {
        public Task<UserResponse> Handle(Query query, CancellationToken ct)
            => verify.ExecuteAsync(query, ct);
    }
}
