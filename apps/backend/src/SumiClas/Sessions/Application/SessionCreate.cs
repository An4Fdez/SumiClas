using MediatR;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Sessions.Application;

/// <summary>
/// Opens a session (login): checks the credentials through <see cref="UserPasswordVerify.Query"/>, starts a new session
/// with its first refresh token and issues the first access token. A command, because the credential check may store a
/// stronger hash.
/// </summary>
public static class SessionCreate
{
    public sealed record Command(string Email, string Password) : IRequest<SessionResponse>, ICommand;

    public sealed class Create(
        IQueryBus queryBus,
        IRefreshTokenGenerator refreshTokenGenerator,
        ISessionRepository repository,
        IAccessTokenGenerator accessTokenGenerator,
        IClock clock) : IUseCase
    {
        public async Task<SessionResponse> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            UserResponse user = await queryBus.Send(new UserPasswordVerify.Query(command.Email, command.Password), ct);

            string refreshToken = refreshTokenGenerator.Generate();
            Session session = Session.Create(user.Id, refreshTokenGenerator.Hash(refreshToken), clock);

            await repository.AddAsync(session, ct);

            AccessToken accessToken = accessTokenGenerator.Generate(user.Id);

            return new SessionResponse(accessToken.Value, accessToken.ExpiresAt, user.Id,
                user.FullName, refreshToken, session.RefreshTokens.Single().ExpiresAt);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, SessionResponse>
    {
        public Task<SessionResponse> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
