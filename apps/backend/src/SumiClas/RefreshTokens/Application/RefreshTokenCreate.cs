using MediatR;
using Microsoft.Extensions.Options;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.RefreshTokens.Application;

/// <summary>
/// Opens a session: checks the credentials through <see cref="UserPasswordVerify.Query"/>, starts a new refresh token
/// family and issues the first access token. A command, because the credential check may store a stronger hash.
/// </summary>
public static class RefreshTokenCreate
{
    public sealed record Command(string Email, string Password) : IRequest<Result<SessionResponse>>, ICommand;

    public sealed class Create(
        IQueryBus queryBus,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokens,
        IAccessTokenGenerator accessTokenGenerator,
        IClock clock,
        IOptions<JwtOptions> jwtOptions) : IUseCase
    {
        public async Task<Result<SessionResponse>> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var user = await queryBus.Send(new UserPasswordVerify.Query(command.Email, command.Password), ct);
            if (user.IsFailure)
            {
                return Result<SessionResponse>.Failure(user.Error!);
            }

            var (raw, hash) = refreshTokenGenerator.Generate();
            var refreshToken = RefreshToken.Create(
                Guid.NewGuid(), user.Value.Id, Guid.NewGuid(), hash, clock, TimeSpan.FromDays(jwtOptions.Value.RefreshTokenDays));
            if (refreshToken.IsFailure)
            {
                return Result<SessionResponse>.Failure(refreshToken.Error!);
            }

            await refreshTokens.AddAsync(refreshToken.Value, ct);

            var accessToken = accessTokenGenerator.Generate(user.Value.Id);
            if (accessToken.IsFailure)
            {
                return Result<SessionResponse>.Failure(accessToken.Error!);
            }

            return Result<SessionResponse>.Success(
                new SessionResponse(accessToken.Value.Value, accessToken.Value.ExpiresAt, user.Value.Id, user.Value.FullName, raw));
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result<SessionResponse>>
    {
        public Task<Result<SessionResponse>> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
