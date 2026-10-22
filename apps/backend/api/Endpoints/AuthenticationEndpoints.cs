using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Api.Endpoints;

public sealed record AccountRegisterRequest(
    Guid Id,
    [StringLength(200)] string FullName,
    [StringLength(320)] string Email,
    [StringLength(128)] string Password);

public sealed record AccountLoginRequest(
    [StringLength(320)] string Email,
    [StringLength(128)] string Password);

public sealed class AuthenticationEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder authentication = app.MapGroup("/auth")
            .WithTags("Authentication")
            .AllowAnonymous();

        RouteGroupBuilder credentials = authentication.MapGroup(string.Empty)
            .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicy);

        credentials.MapPost("/register", async (AccountRegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new UserRegister.Command(request.Id, request.FullName, request.Email, request.Password), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("UserRegister")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        credentials.MapPost("/login", async (
                AccountLoginRequest request,
                ISender sender,
                HttpResponse response,
                IOptions<RefreshCookieOptions> cookieOptions,
                TimeProvider time,
                CancellationToken ct) =>
        {
            SessionResponse session = await sender.Send(new SessionCreate.Command(request.Email, request.Password), ct);

            return Respond(session, response, cookieOptions.Value, time);
        })
            .WithName("SessionCreate")
            .Produces<AccountSessionResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        RouteGroupBuilder session = authentication.MapGroup(string.Empty)
            .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthRefreshPolicy);

        session.MapPost("/refresh", async (
                HttpRequest request,
                HttpResponse response,
                ISender sender,
                IOptions<RefreshCookieOptions> cookieOptions,
                TimeProvider time,
                CancellationToken ct) =>
        {
            RefreshCookieOptions options = cookieOptions.Value;
            string? refreshToken = RefreshCookie.Read(request, options);
            try
            {
                SessionResponse rotated = await sender.Send(new SessionRotate.Command(refreshToken), ct);

                return Respond(rotated, response, options, time);
            }
            catch (SessionExceptions.RefreshTokenReused)
            {
                // The failed rotation rolled back, so the revocation of the stolen session is its own command and
                // its own transaction; it is not atomic with the detection. It must survive the client hanging up:
                // whoever replays a token could otherwise cancel the revocation by aborting the request.
                await sender.Send(new SessionRevoke.Command(refreshToken), CancellationToken.None);

                RefreshCookie.Clear(response, options);
                return DomainProblem.RefreshTokenInvalid();
            }
            catch (AuthenticationExceptions.RefreshTokenInvalid)
            {
                RefreshCookie.Clear(response, options);
                return DomainProblem.RefreshTokenInvalid();
            }
        })
            .WithName("SessionRotate")
            .Produces<AccountSessionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        session.MapPost("/logout", async (
                HttpRequest request,
                HttpResponse response,
                ISender sender,
                IOptions<RefreshCookieOptions> cookieOptions,
                CancellationToken ct) =>
        {
            RefreshCookieOptions options = cookieOptions.Value;
            await sender.Send(new SessionRevoke.Command(RefreshCookie.Read(request, options)), ct);

            RefreshCookie.Clear(response, options);
            return Results.NoContent();
        })
            .WithName("SessionRevoke")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }

    private static IResult Respond(SessionResponse session, HttpResponse response, RefreshCookieOptions options, TimeProvider time)
    {
        RefreshCookie.Write(response, session.RefreshTokenRaw, session.RefreshTokenExpiresAt, time.GetUtcNow(), options);
        return Results.Ok(new AccountSessionResponse(session.AccessToken, session.ExpiresAt, session.UserId, session.FullName));
    }
}
