using MediatR;
using SumiClas.Api.Http;
using SumiClas.Users.Application;

namespace SumiClas.Api.Endpoints;

public sealed record UserRegisterRequest(Guid Id, string FullName, string Email);

public sealed class UsersEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users");

        users.MapPost("/", async (UserRegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UserRegister.Command(request.Id, request.FullName, request.Email), ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
