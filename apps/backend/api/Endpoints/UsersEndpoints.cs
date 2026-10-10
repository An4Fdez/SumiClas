using MediatR;
using SumiClas.Api.Http;
using SumiClas.Students.Application;
using SumiClas.Users.Application;

namespace SumiClas.Api.Endpoints;

public sealed class UsersEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users").WithTags("Users");

        users.MapPut(
            "/{userId:guid}/class-representative-roles/{classId:guid}",
            async (Guid userId, Guid classId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new UserClassRepresentativeAssign.Command(userId, classId), ct);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblem();
            })
            .WithName("UserClassRepresentativeAssign")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        users.MapDelete(
            "/me/class-representative-roles/{classId:guid}",
            async (Guid classId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new UserClassRepresentativeResign.Command(classId), ct);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblem();
            })
            .WithName("UserClassRepresentativeResign")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        users.MapGet("/me", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new UserProfileFinderCurrent.Query(), ct)).ToOk())
            .WithName("UserProfileFinderCurrent")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        users.MapGet("/me/students", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new StudentFinderByCurrentGuardian.Query(), ct)).ToOk())
            .WithName("StudentFinderByCurrentGuardian")
            .Produces<IReadOnlyCollection<ChildResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/classes/{classId:guid}/members", async (Guid classId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UserClassMemberFinderByClass.Query(classId), ct)).ToOk())
            .WithTags("Users")
            .WithName("UserClassMemberFinderByClass")
            .Produces<IReadOnlyCollection<UserClassMemberResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
