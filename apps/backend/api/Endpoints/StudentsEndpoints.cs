using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;

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
        var students = app.MapGroup("/students").WithTags("Students");

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
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequiresSession()
            .Maps<StudentExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.NameRequired>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.FirstSurnameRequired>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.FirstGuardianRequired>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.InvalidGuardianRole>(StatusCodes.Status400BadRequest)
            .ConflictsOnSave();

        students.MapPost("/{studentId:guid}/guardians", async (Guid studentId, StudentGuardianAddRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new StudentGuardianAdd.Command(studentId, request.GuardianId, request.Role), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("StudentGuardianAdd")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequiresSession()
            .Maps<StudentExceptions.InvalidGuardianRole>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.GuardianIdRequired>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.GuardianAlreadyLinked>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<StudentExceptions.GuardianChangeForbidden>(StatusCodes.Status403Forbidden)
            .ConflictsOnSave();

        students.MapDelete("/{studentId:guid}/guardians/{guardianId:guid}", async (Guid studentId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new StudentGuardianRemove.Command(studentId, guardianId), ct);

            return Results.NoContent();
        })
            .WithName("StudentGuardianRemove")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequiresSession()
            .Maps<StudentExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<StudentExceptions.GuardianChangeForbidden>(StatusCodes.Status403Forbidden)
            .Maps<StudentExceptions.GuardianNotLinked>(StatusCodes.Status400BadRequest)
            .Maps<StudentExceptions.AtLeastOneGuardianRequired>(StatusCodes.Status400BadRequest)
            .ConflictsOnSave();
    }
}
