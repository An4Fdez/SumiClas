using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Bootstrap;
using SumiClas.Api.Http;
using SumiClas.Sessions.Application;
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
        var authentication = app.MapGroup("/auth")
            .WithTags("Authentication")
            .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicy);

        authentication.MapPost("/register", async (AccountRegisterRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UserRegister.Command(request.Id, request.FullName, request.Email, request.Password), ct)).ToCreated())
            .WithName("UserRegister")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        authentication.MapPost("/login", async (AccountLoginRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SessionCreate.Command(request.Email, request.Password), ct);
            return result.IsSuccess
                ? Results.Ok(new AccountResponse(result.Value.UserId, result.Value.FullName))
                : result.ToProblem();
        })
            .WithName("SessionCreate")
            .Produces<AccountResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }
}
