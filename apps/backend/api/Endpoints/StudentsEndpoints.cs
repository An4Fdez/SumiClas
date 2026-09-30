using MediatR;
using SumiClas.Api.Http;
using SumiClas.Students.Application;

namespace SumiClas.Api.Endpoints;

public sealed record StudentRegisterRequest(
    Guid Id,
    string FirstName,
    string FirstSurname,
    string? SecondSurname,
    Guid FirstGuardianId,
    string FirstGuardianRole);

public sealed class StudentsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var students = app.MapGroup("/students");

        students.MapPost("/", async (StudentRegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new StudentRegister.Command(
                    request.Id,
                    request.FirstName,
                    request.FirstSurname,
                    request.SecondSurname,
                    request.FirstGuardianId,
                    request.FirstGuardianRole),
                ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
