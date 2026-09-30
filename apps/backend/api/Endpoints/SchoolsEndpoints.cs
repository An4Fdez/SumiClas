using MediatR;
using SumiClas.Api.Http;
using SumiClas.Schools.Application;

namespace SumiClas.Api.Endpoints;

public sealed record RegisterSchoolRequest(Guid Id, string Name, string OfficialCode, string? Address);

public static class SchoolsEndpoints
{
    public static void MapSchoolsEndpoints(this WebApplication app)
    {
        var schools = app.MapGroup("/schools");

        schools.MapPost("/", async (RegisterSchoolRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new RegisterSchool.Command(request.Id, request.Name, request.OfficialCode, request.Address), ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
