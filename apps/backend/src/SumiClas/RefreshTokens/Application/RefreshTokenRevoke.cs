using MediatR;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.RefreshTokens.Application;

/// <summary>Closes the whole session family of a refresh token. Always succeeds: logging out twice, or with garbage, is harmless.</summary>
public static class RefreshTokenRevoke
{
    public sealed record Command(string? RawRefreshToken) : IRequest<Result>, ICommand;

    public sealed class Revoke(IRefreshTokenRepository refreshTokens, IRefreshTokenGenerator refreshTokenGenerator, IClock clock) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(command.RawRefreshToken))
            {
                return Result.Success();
            }

            var hash = refreshTokenGenerator.Hash(command.RawRefreshToken);
            var found = await refreshTokens.SearchAsync(new RefreshTokenSpecification(tokenHash: hash), ct);
            var current = found.FirstOrDefault();
            if (current is not null)
            {
                await RefreshTokenRotate.Rotate.RevokeFamilyAsync(current.FamilyId, clock, refreshTokens, ct);
            }

            return Result.Success();
        }
    }

    public sealed class Handler(Revoke revoke) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => revoke.ExecuteAsync(command, ct);
    }
}
