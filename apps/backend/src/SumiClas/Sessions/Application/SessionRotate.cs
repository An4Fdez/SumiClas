using MediatR;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Sessions.Application;

/// <summary>
/// Rotates a refresh token. It only fails for technical reasons: an unusable token is <see cref="SessionRotateOutcome.Invalid"/>
/// and a replayed one is <see cref="SessionRotateOutcome.Reused"/>, both successful results, because a failed result would
/// roll back the revocation of the session.
/// </summary>
public static class SessionRotate
{
    public sealed record Command(string? RefreshToken) : IRequest<Result<SessionRotateOutcome>>, ICommand;

    public sealed class Rotate(
        ISessionRepository repository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAccessTokenGenerator accessTokenGenerator,
        IQueryBus queryBus,
        IClock clock) : IUseCase
    {
        public async Task<Result<SessionRotateOutcome>> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return Done(new SessionRotateOutcome.Invalid());
            }

            var hash = refreshTokenGenerator.Hash(command.RefreshToken);
            var found = await repository.SearchAsync(new SessionSpecification(tokenHash: hash), ct);
            var session = found.FirstOrDefault();
            if (session is null)
            {
                return Done(new SessionRotateOutcome.Invalid());
            }

            var user = await queryBus.Send(new UserFinderByIdOrNull.Query(session.UserId), ct);
            if (user is null)
            {
                return Done(new SessionRotateOutcome.Invalid());
            }

            var refreshToken = refreshTokenGenerator.Generate();
            var rotation = session.Rotate(hash, refreshTokenGenerator.Hash(refreshToken), clock);
            if (rotation.IsFailure)
            {
                return Result<SessionRotateOutcome>.Failure(rotation.Error!);
            }

            switch (rotation.Value)
            {
                case SessionRotation.Rotated rotated:
                    var accessToken = accessTokenGenerator.Generate(session.UserId);
                    if (accessToken.IsFailure)
                    {
                        return Result<SessionRotateOutcome>.Failure(accessToken.Error!);
                    }

                    return Done(new SessionRotateOutcome.Rotated(
                        new SessionResponse(accessToken.Value.Value, accessToken.Value.ExpiresAt, session.UserId,
                            user.FullName, refreshToken, rotated.NewToken.ExpiresAt)));
                case SessionRotation.Reused:
                    return Done(new SessionRotateOutcome.Reused());
                default:
                    return Done(new SessionRotateOutcome.Invalid());
            }
        }

        private static Result<SessionRotateOutcome> Done(SessionRotateOutcome outcome) =>
            Result<SessionRotateOutcome>.Success(outcome);
    }

    public sealed class Handler(Rotate rotate) : IRequestHandler<Command, Result<SessionRotateOutcome>>
    {
        public Task<Result<SessionRotateOutcome>> Handle(Command command, CancellationToken ct)
            => rotate.ExecuteAsync(command, ct);
    }
}