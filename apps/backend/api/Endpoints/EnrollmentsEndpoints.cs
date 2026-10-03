using MediatR;
using SumiClas.Api.Http;
using SumiClas.Enrollments.Application;

namespace SumiClas.Api.Endpoints;

public sealed record EnrollmentCreateRequest(Guid Id, Guid StudentId, Guid ClassId);

public sealed class EnrollmentsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var enrollments = app.MapGroup("/enrollments");

        enrollments.MapPost("/", async (EnrollmentCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new EnrollmentCreate.Command(request.Id, request.StudentId, request.ClassId), ct);

            return result.ToCreated();
        });

        enrollments.MapPost("/{enrollmentId:guid}/confirm", async (Guid enrollmentId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new EnrollmentConfirm.Command(enrollmentId), ct);

            return result.ToNoContent();
        });
    }
}
