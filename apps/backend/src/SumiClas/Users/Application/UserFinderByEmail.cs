using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserFinderByEmail
{
    /// <summary>Fails with <see cref="UserErrors.InvalidEmailFormat"/> for a malformed email and with <see cref="UserErrors.NotFound"/> when nobody has it.</summary>
    public sealed record Query(string Email) : IQuery<Result<UserResponse>>;

    public sealed class Finder(IUserRepository users) : IUseCase
    {
        public async Task<Result<UserResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var email = Email.Create(query.Email);
            if (email.IsFailure)
            {
                return Result<UserResponse>.Failure(email.Error!);
            }

            var found = await users.SearchAsync(new UserSpecification(email: email.Value), ct);
            var user = found.FirstOrDefault();
            return user is null
                ? Result<UserResponse>.Failure(UserErrors.NotFound)
                : Result<UserResponse>.Success(new UserResponse(user.Id, user.FullName));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<UserResponse>>
    {
        public Task<Result<UserResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
