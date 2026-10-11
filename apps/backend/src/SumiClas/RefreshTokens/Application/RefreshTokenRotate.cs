using MediatR;
using Microsoft.Extensions.Options;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.RefreshTokens.Application;

/// <summary>
/// Rotates a refresh token. It only fails for technical reasons: an unusable token is <see cref="RefreshOutcome.Invalid"/>
/// and a replayed one is <see cref="RefreshOutcome.Reused"/>, both successful results, because a failed result would roll
/// back the revocation of the family.
/// </summary>
public static class RefreshTokenRotate
{
    public sealed record Command(string? RawRefreshToken) : IRequest<Result<RefreshOutcome>>, ICommand;

    public sealed class Rotate(
        IRefreshTokenRepository refreshTokens,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAccessTokenGenerator accessTokenGenerator,
        IQueryBus queryBus,
        IClock clock,
        IOptions<JwtOptions> jwtOptions) : IUseCase
    {
        public async Task<Result<RefreshOutcome>> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(command.RawRefreshToken))
            {
                return Done(new RefreshOutcome.Invalid());
            }

            var options = jwtOptions.Value;
            var hash = refreshTokenGenerator.Hash(command.RawRefreshToken);
            var found = await refreshTokens.SearchAsync(new RefreshTokenSpecification(tokenHash: hash), ct);
            var current = found.FirstOrDefault();
            if (current is null)
            {
                return Done(new RefreshOutcome.Invalid());
            }

            if (current.ReplacedAt is not null)
            {
                if (current.WasReplacedWithin(clock, TimeSpan.FromSeconds(options.RefreshGraceSeconds)))
                {
                    return Done(new RefreshOutcome.Invalid());
                }

                await RevokeFamilyAsync(current.FamilyId, clock, refreshTokens, ct);
                return Done(new RefreshOutcome.Reused());
            }

            if (!current.IsActive(clock))
            {
                return Done(new RefreshOutcome.Invalid());
            }

            var user = await queryBus.Send(new UserFinderById.Query(current.UserId), ct);
            if (user.IsFailure)
            {
                return Done(new RefreshOutcome.Invalid());
            }

            var (raw, newHash) = refreshTokenGenerator.Generate();
            var successor = RefreshToken.Create(
                Guid.NewGuid(), current.UserId, current.FamilyId, newHash, clock, TimeSpan.FromDays(options.RefreshTokenDays));
            if (successor.IsFailure)
            {
                return Result<RefreshOutcome>.Failure(successor.Error!);
            }

            var replaced = current.MarkReplaced(successor.Value.Id, clock);
            if (replaced.IsFailure)
            {
                return Result<RefreshOutcome>.Failure(replaced.Error!);
            }

            await refreshTokens.AddAsync(successor.Value, ct);
            var accessToken = accessTokenGenerator.Generate(current.UserId);
            if (accessToken.IsFailure)
            {
                return Result<RefreshOutcome>.Failure(accessToken.Error!);
            }

            return Done(new RefreshOutcome.Rotated(
                new SessionResponse(accessToken.Value.Value, accessToken.Value.ExpiresAt, current.UserId, user.Value.FullName, raw)));
        }

        internal static async Task RevokeFamilyAsync(Guid familyId, IClock clock, IRefreshTokenRepository refreshTokens, CancellationToken ct)
        {
            var family = await refreshTokens.SearchAsync(new RefreshTokenSpecification(familyId: familyId), ct);
            foreach (var token in family)
            {
                token.Revoke(clock);
            }
        }

        private static Result<RefreshOutcome> Done(RefreshOutcome outcome) => Result<RefreshOutcome>.Success(outcome);
    }

    public sealed class Handler(Rotate rotate) : IRequestHandler<Command, Result<RefreshOutcome>>
    {
        public Task<Result<RefreshOutcome>> Handle(Command command, CancellationToken ct)
            => rotate.ExecuteAsync(command, ct);
    }
}
