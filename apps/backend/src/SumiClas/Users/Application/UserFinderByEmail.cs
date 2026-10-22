using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserFinderByEmail
{
    /// <summary>Throws <see cref="UserExceptions.InvalidEmailFormat"/> for a malformed email and <see cref="UserExceptions.NotFound"/> when nobody has it.</summary>
    public sealed record Query(string Email) : IQuery<UserResponse>;

    public sealed class Finder(IUserRepository repository) : IUseCase
    {
        public async Task<UserResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var email = Email.Create(query.Email);

            var found = await repository.SearchAsync(new UserSpecification(email: email), ct);
            var user = found.FirstOrDefault() ?? throw new UserExceptions.NotFound();
            return new UserResponse(user.Id, user.FullName);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, UserResponse>
    {
        public Task<UserResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
