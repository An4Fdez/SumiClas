using MediatR;
using SumiClas.Api.Http;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record EnrollmentCreateRequest(Guid Id, Guid StudentId, Guid ClassId);

public sealed record EnrollmentSuggestedCreateRequest(Guid PreviousClassId);

public sealed class EnrollmentsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var enrollments = app.MapGroup("/enrollments").WithTags("Enrollments");

        enrollments.MapPost("/", async (EnrollmentCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new EnrollmentCreate.Command(request.Id, request.StudentId, request.ClassId), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("EnrollmentCreate")
            .Produces(StatusCodes.Status201Created)
            .Maps<EnrollmentExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.StudentRequired>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.ClassRequired>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.AlreadyExists>(StatusCodes.Status409Conflict)
            .RequiresSession()
            .RequiresStudentGuardian()
            .RequiresClass()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        enrollments.MapPost("/{enrollmentId:guid}/confirm", async (Guid enrollmentId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new EnrollmentConfirm.Command(enrollmentId), ct);

            return Results.NoContent();
        })
            .WithName("EnrollmentConfirm")
            .Produces(StatusCodes.Status204NoContent)
            .Maps<EnrollmentExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<EnrollmentExceptions.AlreadyConfirmed>(StatusCodes.Status409Conflict)
            .Maps<EnrollmentExceptions.AlreadyExists>(StatusCodes.Status409Conflict)
            .RequiresSession()
            .RequiresStudentGuardian()
            .RequiresClass()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        app.MapPost("/classes/{classId:guid}/suggested-enrollments", async (Guid classId, EnrollmentSuggestedCreateRequest request, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new EnrollmentSuggestedCreate.Command(classId, request.PreviousClassId), ct)))
            .WithTags("Enrollments")
            .WithName("EnrollmentSuggestedCreate")
            .Produces<EnrollmentSuggestedCreateResponse>(StatusCodes.Status200OK)
            .Maps<EnrollmentExceptions.ClassRequired>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.SameClass>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.SameSchoolYear>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.DifferentSchool>(StatusCodes.Status400BadRequest)
            .Maps<EnrollmentExceptions.SuggestForbidden>(StatusCodes.Status403Forbidden)
            .RequiresSession()
            .RequiresClass()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();
    }
}
