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
                await sender.Send(new UserClassRepresentativeAssign.Command(userId, classId), ct);

                return Results.NoContent();
            })
            .WithName("UserClassRepresentativeAssign")
            .Produces(StatusCodes.Status204NoContent);

        users.MapDelete(
            "/me/class-representative-roles/{classId:guid}",
            async (Guid classId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UserClassRepresentativeResign.Command(classId), ct);

                return Results.NoContent();
            })
            .WithName("UserClassRepresentativeResign")
            .Produces(StatusCodes.Status204NoContent);

        users.MapGet("/me", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UserProfileFinderCurrent.Query(), ct)))
            .WithName("UserProfileFinderCurrent")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK);

        users.MapGet("/me/students", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new StudentFinderByCurrentGuardian.Query(), ct)))
            .WithName("StudentFinderByCurrentGuardian")
            .Produces<IReadOnlyCollection<ChildResponse>>(StatusCodes.Status200OK);

        app.MapGet("/classes/{classId:guid}/members", async (Guid classId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UserClassMemberFinderByClass.Query(classId), ct)))
            .WithTags("Users")
            .WithName("UserClassMemberFinderByClass")
            .Produces<IReadOnlyCollection<UserClassMemberResponse>>(StatusCodes.Status200OK);
    }
}
