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

public sealed record StudentNameChangeRequest(
    [StringLength(100)] string FirstName,
    [StringLength(100)] string FirstSurname,
    [StringLength(100)] string? SecondSurname);

public sealed record StudentGuardianAddRequest(Guid GuardianId, string Role);

public sealed class StudentsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder students = app.MapGroup("/students").WithTags("Students");

        students.MapPost("/", async (StudentRegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new StudentRegister.Command(
                    request.Id,
                    request.FirstName,
                    request.FirstSurname,
                    request.SecondSurname,
                    request.FirstGuardianRole),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("StudentRegister")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        students.MapPut("/{studentId:guid}", async (Guid studentId, StudentNameChangeRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new StudentNameChange.Command(studentId, request.FirstName, request.FirstSurname, request.SecondSurname),
                ct);

            return Results.NoContent();
        })
            .WithName("StudentNameChange")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        students.MapPost("/{studentId:guid}/guardians", async (Guid studentId, StudentGuardianAddRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new StudentGuardianAdd.Command(studentId, request.GuardianId, request.Role), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("StudentGuardianAdd")
            .Produces(StatusCodes.Status201Created);

        students.MapDelete("/{studentId:guid}/guardians/{guardianId:guid}", async (Guid studentId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new StudentGuardianRemove.Command(studentId, guardianId), ct);

            return Results.NoContent();
        })
            .WithName("StudentGuardianRemove")
            .Produces(StatusCodes.Status204NoContent);
    }
}
