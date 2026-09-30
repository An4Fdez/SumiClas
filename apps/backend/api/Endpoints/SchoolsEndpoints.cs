using MediatR;
using SumiClas.Api.Http;
using SumiClas.Schools.Application;

namespace SumiClas.Api.Endpoints;

public sealed record SchoolRegisterRequest(Guid Id, string Name, string OfficialCode, string? Address);

public sealed class SchoolsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var schools = app.MapGroup("/schools");

        schools.MapPost("/", async (SchoolRegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SchoolRegister.Command(request.Id, request.Name, request.OfficialCode, request.Address), ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
