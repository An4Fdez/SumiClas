using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Subjects.Application;

namespace SumiClas.Api.Endpoints;

public sealed record SubjectCreateRequest(Guid Id, Guid ClassId, [StringLength(100)] string Label);

public sealed class SubjectsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var subjects = app.MapGroup("/subjects");

        subjects.MapPost("/", async (SubjectCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SubjectCreate.Command(request.Id, request.ClassId, request.Label), ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
