using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Students.Application;

namespace SumiClas.Api.Endpoints;

public sealed record StudentRegisterRequest(
    Guid Id,
    [StringLength(100)] string FirstName,
    [StringLength(100)] string FirstSurname,
    [StringLength(100)] string? SecondSurname,
    string FirstGuardianRole);

public sealed record StudentGuardianAddRequest(Guid GuardianId, string Role);

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
                    request.FirstGuardianRole),
                ct);

            return result.ToCreated();
        })
            .WithName("StudentRegister")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        students.MapPost("/{studentId:guid}/guardians", async (Guid studentId, StudentGuardianAddRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new StudentGuardianAdd.Command(studentId, request.GuardianId, request.Role), ct);

            return result.ToCreated();
        })
            .WithName("StudentGuardianAdd")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        students.MapDelete("/{studentId:guid}/guardians/{guardianId:guid}", async (Guid studentId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new StudentGuardianRemove.Command(studentId, guardianId), ct);

            return result.ToNoContent();
        })
            .WithName("StudentGuardianRemove")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
