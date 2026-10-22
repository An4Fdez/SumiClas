using MediatR;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Sessions.Application;

/// <summary>
/// Rotates a refresh token. An unusable token throws <see cref="AuthenticationExceptions.RefreshTokenInvalid"/>. A replayed
/// one throws <see cref="SessionExceptions.RefreshTokenReused"/>: the exception rolls the transaction back, so the
/// revocation of the session is not saved and whoever catches it has to send <see cref="SessionRevoke.Command"/>.
/// </summary>
public static class SessionRotate
{
    public sealed record Command(string? RefreshToken) : IRequest<SessionResponse>, ICommand;

    public sealed class Rotate(
        ISessionRepository repository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAccessTokenGenerator accessTokenGenerator,
        IQueryBus queryBus,
        IClock clock) : IUseCase
    {
        public async Task<SessionResponse> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                throw new AuthenticationExceptions.RefreshTokenInvalid();
            }

            var hash = refreshTokenGenerator.Hash(command.RefreshToken);
            var found = await repository.SearchAsync(new SessionSpecification(tokenHash: hash), ct);
            var session = found.FirstOrDefault() ?? throw new AuthenticationExceptions.RefreshTokenInvalid();

            var user = await queryBus.Send(new UserFinderByIdOrNull.Query(session.UserId), ct)
                ?? throw new AuthenticationExceptions.RefreshTokenInvalid();

            var refreshToken = refreshTokenGenerator.Generate();
            var rotation = session.Rotate(hash, refreshTokenGenerator.Hash(refreshToken), clock);

            switch (rotation)
            {
                case SessionRotation.Rotated rotated:
                    var accessToken = accessTokenGenerator.Generate(session.UserId);

                    return new SessionResponse(accessToken.Value, accessToken.ExpiresAt, session.UserId,
                        user.FullName, refreshToken, rotated.NewToken.ExpiresAt);
                case SessionRotation.Reused:
                    throw new SessionExceptions.RefreshTokenReused();
                default:
                    throw new AuthenticationExceptions.RefreshTokenInvalid();
            }
        }
    }

    public sealed class Handler(Rotate rotate) : IRequestHandler<Command, SessionResponse>
    {
        public Task<SessionResponse> Handle(Command command, CancellationToken ct)
            => rotate.ExecuteAsync(command, ct);
    }
}
