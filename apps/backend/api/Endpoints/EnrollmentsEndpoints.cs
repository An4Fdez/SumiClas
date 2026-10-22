using MediatR;
using SumiClas.Api.Http;
using SumiClas.Enrollments.Application;

namespace SumiClas.Api.Endpoints;

public sealed record EnrollmentCreateRequest(Guid Id, Guid StudentId, Guid ClassId);

public sealed record EnrollmentSuggestedCreateRequest(Guid PreviousClassId);

public sealed class EnrollmentsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder enrollments = app.MapGroup("/enrollments").WithTags("Enrollments");

        enrollments.MapPost("/", async (EnrollmentCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new EnrollmentCreate.Command(request.Id, request.StudentId, request.ClassId), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("EnrollmentCreate")
            .Produces(StatusCodes.Status201Created);

        enrollments.MapPost("/{enrollmentId:guid}/confirm", async (Guid enrollmentId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new EnrollmentConfirm.Command(enrollmentId), ct);

            return Results.NoContent();
        })
            .WithName("EnrollmentConfirm")
            .Produces(StatusCodes.Status204NoContent);

        app.MapPost("/classes/{classId:guid}/suggested-enrollments", async (Guid classId, EnrollmentSuggestedCreateRequest request, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new EnrollmentSuggestedCreate.Command(classId, request.PreviousClassId), ct)))
            .WithTags("Enrollments")
            .WithName("EnrollmentSuggestedCreate")
            .Produces<EnrollmentSuggestedCreateResponse>(StatusCodes.Status200OK);
    }
}
