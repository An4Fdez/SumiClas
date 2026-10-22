using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.CalendarEvents.Domain;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record ClassExpenseCreateRequest(
    Guid Id,
    Guid ClassId,
    [StringLength(200)] string Item,
    [Range(0.01, 99999999.99)] decimal TotalAmount,
    [Required] IReadOnlyCollection<Guid> ParticipantIds,
    Guid? RelatedEventId);

public sealed class ClassExpensesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var classExpenses = app.MapGroup("/class-expenses").WithTags("ClassExpenses");

        classExpenses.MapPost("/", async (ClassExpenseCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new ClassExpenseCreate.Command(
                    request.Id,
                    request.ClassId,
                    request.Item,
                    request.TotalAmount,
                    request.ParticipantIds,
                    request.RelatedEventId),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("ClassExpenseCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<ClassExpenseExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.ClassRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.ItemRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.TotalAmountMustBePositive>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.AtLeastOneParticipantRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.AuthorRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.ParticipantNotInClass>(StatusCodes.Status400BadRequest)
            .Maps<ClassExpenseExceptions.ParticipantNotFound>(StatusCodes.Status404NotFound)
            .Maps<CalendarEventExceptions.NotFound>(StatusCodes.Status404NotFound)
            .RequiresSession()
            .RequiresUser()
            .RequiresClass()
            .RequiresClassRepresentative()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        classExpenses.MapPut("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassExpensePaidMark.Command(classExpenseId, guardianId), ct);

            return Results.NoContent();
        })
            .WithName("ClassExpensePaidMark")
            .Produces(StatusCodes.Status204NoContent)
            .Maps<ClassExpenseExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<ClassExpenseExceptions.NotAParticipant>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership()
            .RequiresClassRepresentative()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        classExpenses.MapDelete("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassExpenseUnpaidMark.Command(classExpenseId, guardianId), ct);

            return Results.NoContent();
        })
            .WithName("ClassExpenseUnpaidMark")
            .Produces(StatusCodes.Status204NoContent)
            .Maps<ClassExpenseExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<ClassExpenseExceptions.NotAParticipant>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership()
            .RequiresClassRepresentative()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        classExpenses.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ClassExpenseFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("ClassExpenseFinderByClass")
            .Produces<PagedResponse<ClassExpenseResponse>>(StatusCodes.Status200OK)
            .Maps<PaginationExceptions.InvalidLimit>(StatusCodes.Status400BadRequest)
            .Maps<PaginationExceptions.InvalidCursor>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership();
    }
}
