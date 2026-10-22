using MediatR;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Application;

/// <summary>Closes the session of a refresh token. Never fails: logging out twice, or with garbage, is harmless.</summary>
public static class SessionRevoke
{
    public sealed record Command(string? RefreshToken) : IRequest, ICommand;

    public sealed class Revoke(ISessionRepository repository, IRefreshTokenGenerator refreshTokenGenerator, IClock clock) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (command.RefreshToken is null)
            {
                return;
            }

            var hash = refreshTokenGenerator.Hash(command.RefreshToken);
            var found = await repository.SearchAsync(new SessionSpecification(tokenHash: hash), ct);
            found.FirstOrDefault()?.Revoke(clock);
        }
    }

    public sealed class Handler(Revoke revoke) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => revoke.ExecuteAsync(command, ct);
    }
}
