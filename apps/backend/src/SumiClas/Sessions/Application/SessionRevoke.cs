using MediatR;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Application;

/// <summary>Closes the session of a refresh token. Always succeeds: logging out twice, or with garbage, is harmless.</summary>
public static class SessionRevoke
{
    public sealed record Command(string? RefreshToken) : IRequest<Result>, ICommand;

    public sealed class Revoke(ISessionRepository repository, IRefreshTokenGenerator refreshTokenGenerator, IClock clock) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (command.RefreshToken is null)
            {
                return Result.Success();
            }

            var hash = refreshTokenGenerator.Hash(command.RefreshToken);
            var found = await repository.SearchAsync(new SessionSpecification(tokenHash: hash), ct);
            found.FirstOrDefault()?.Revoke(clock);

            return Result.Success();
        }
    }

    public sealed class Handler(Revoke revoke) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => revoke.ExecuteAsync(command, ct);
    }
}
