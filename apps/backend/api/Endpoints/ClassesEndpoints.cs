using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Classes.Application;

namespace SumiClas.Api.Endpoints;

public sealed record ClassCreateRequest(Guid Id, Guid SchoolYearId, [StringLength(100)] string Name);

public sealed record ClassRenameRequest([StringLength(100)] string Name);

public sealed class ClassesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var classes = app.MapGroup("/classes");

        classes.MapPost("/", async (ClassCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClassCreate.Command(request.Id, request.SchoolYearId, request.Name), ct);

            return result.ToCreated();
        });

        classes.MapPut("/{classId:guid}/name", async (Guid classId, ClassRenameRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClassRename.Command(classId, request.Name), ct);

            return result.ToNoContent();
        });
    }
}
