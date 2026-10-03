using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Users.Application;

namespace SumiClas.Api.Endpoints;

public sealed record UserRegisterRequest(Guid Id, [StringLength(200)] string FullName, [StringLength(320)] string Email);

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

        users.MapPut(
            "/{userId:guid}/class-representative-roles/{classId:guid}",
            async (Guid userId, Guid classId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new UserClassRepresentativeAssign.Command(userId, classId), ct);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblem();
            });

        users.MapDelete(
            "/me/class-representative-roles/{classId:guid}",
            async (Guid classId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new UserClassRepresentativeResign.Command(classId), ct);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblem();
            });
    }
}
