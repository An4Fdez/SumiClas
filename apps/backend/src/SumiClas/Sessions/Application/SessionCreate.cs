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
    public sealed record Command(string Email, string Password) : IRequest<Result<SessionResponse>>, ICommand;

    public sealed class Create(
        IQueryBus queryBus,
        IRefreshTokenGenerator refreshTokenGenerator,
        ISessionRepository repository,
        IAccessTokenGenerator accessTokenGenerator,
        IClock clock) : IUseCase
    {
        public async Task<Result<SessionResponse>> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var user = await queryBus.Send(new UserPasswordVerify.Query(command.Email, command.Password), ct);

            var refreshToken = refreshTokenGenerator.Generate();
            var session = Session.Create(user.Id, refreshTokenGenerator.Hash(refreshToken), clock);

            if (session.IsFailure)
            {
                return Result<SessionResponse>.Failure(session.Error!);
            }

            await repository.AddAsync(session.Value, ct);

            var accessToken = accessTokenGenerator.Generate(user.Id);
            if (accessToken.IsFailure)
            {
                return Result<SessionResponse>.Failure(accessToken.Error!);
            }

            return Result<SessionResponse>.Success(
                new SessionResponse(accessToken.Value.Value, accessToken.Value.ExpiresAt, user.Id,
                    user.FullName, refreshToken, session.Value.RefreshTokens.Single().ExpiresAt));
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result<SessionResponse>>
    {
        public Task<Result<SessionResponse>> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}