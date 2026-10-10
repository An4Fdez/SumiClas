using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Bootstrap;
using SumiClas.Api.Http;
using SumiClas.Authentication.Application;
using SumiClas.Authentication.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record AccountRegisterRequest(
    Guid Id,
    [StringLength(200)] string FullName,
    [StringLength(320)] string Email,
    [StringLength(PasswordPolicy.MaxLength)] string Password);

public sealed record AccountLoginRequest(
    [StringLength(320)] string Email,
    [StringLength(PasswordPolicy.MaxLength)] string Password);

public sealed class AuthenticationEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var authentication = app.MapGroup("/auth")
            .WithTags("Authentication")
            .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicy);

        authentication.MapPost("/register", async (AccountRegisterRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AccountRegister.Command(request.Id, request.FullName, request.Email, request.Password), ct)).ToCreated())
            .WithName("AccountRegister")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        authentication.MapPost("/login", async (AccountLoginRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AccountLogin.Command(request.Email, request.Password), ct)).ToOk())
            .WithName("AccountLogin")
            .Produces<AccountResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }
}
