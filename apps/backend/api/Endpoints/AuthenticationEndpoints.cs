using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

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
        var authentication = app.MapGroup("/auth")
            .WithTags("Authentication")
            .AllowAnonymous();

        var credentials = authentication.MapGroup(string.Empty)
            .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicy);

        credentials.MapPost("/register", async (AccountRegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new UserRegister.Command(request.Id, request.FullName, request.Email, request.Password), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("UserRegister")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .Maps<AuthenticationExceptions.PasswordTooWeak>(StatusCodes.Status400BadRequest)
            .Maps<UserExceptions.InvalidEmailFormat>(StatusCodes.Status400BadRequest)
            .Maps<UserExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<UserExceptions.FullNameRequired>(StatusCodes.Status400BadRequest)
            .Maps<UserExceptions.EmailAlreadyRegistered>(StatusCodes.Status409Conflict)
            .ConflictsOnSave();

        credentials.MapPost("/login", async (
                AccountLoginRequest request,
                ISender sender,
                HttpResponse response,
                IOptions<RefreshCookieOptions> cookieOptions,
                TimeProvider time,
                CancellationToken ct) =>
        {
            var session = await sender.Send(new SessionCreate.Command(request.Email, request.Password), ct);

            return Respond(session, response, cookieOptions.Value, time);
        })
            .WithName("SessionCreate")
            .Produces<AccountSessionResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .Maps<AuthenticationExceptions.InvalidCredentials>(StatusCodes.Status401Unauthorized)
            .ConflictsOnSave();

        var session = authentication.MapGroup(string.Empty)
            .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthRefreshPolicy);

        session.MapPost("/refresh", async (
                HttpRequest request,
                HttpResponse response,
                ISender sender,
                IOptions<RefreshCookieOptions> cookieOptions,
                TimeProvider time,
                CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var refreshToken = RefreshCookie.Read(request, options);
            try
            {
                var rotated = await sender.Send(new SessionRotate.Command(refreshToken), ct);

                return Respond(rotated, response, options, time);
            }
            catch (SessionExceptions.RefreshTokenReused)
            {
                // The failed rotation rolled back, so the revocation of the stolen session is its own command and
                // its own transaction; it is not atomic with the detection. It must survive the client hanging up:
                // whoever replays a token could otherwise cancel the revocation by aborting the request.
                await sender.Send(new SessionRevoke.Command(refreshToken), CancellationToken.None);

                RefreshCookie.Clear(response, options);
                return ResultExtensions.RefreshTokenInvalidProblem();
            }
            catch (AuthenticationExceptions.RefreshTokenInvalid)
            {
                RefreshCookie.Clear(response, options);
                return ResultExtensions.RefreshTokenInvalidProblem();
            }
        })
            .WithName("SessionRotate")
            .Produces<AccountSessionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .Maps<AuthenticationExceptions.RefreshTokenInvalid>(StatusCodes.Status401Unauthorized)
            .ConflictsOnSave();

        session.MapPost("/logout", async (
                HttpRequest request,
                HttpResponse response,
                ISender sender,
                IOptions<RefreshCookieOptions> cookieOptions,
                CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            await sender.Send(new SessionRevoke.Command(RefreshCookie.Read(request, options)), ct);

            RefreshCookie.Clear(response, options);
            return Results.NoContent();
        })
            .WithName("SessionRevoke")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ConflictsOnSave();
    }

    private static IResult Respond(SessionResponse session, HttpResponse response, RefreshCookieOptions options, TimeProvider time)
    {
        RefreshCookie.Write(response, session.RefreshTokenRaw, session.RefreshTokenExpiresAt, time.GetUtcNow(), options);
        return Results.Ok(new AccountSessionResponse(session.AccessToken, session.ExpiresAt, session.UserId, session.FullName));
    }
}
