using MediatR;
using SumiClas.Schools.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Application;

public static class SchoolFinderById
{
    public sealed record Query(Guid SchoolId) : IQuery<Result<SchoolResponse>>;

    public sealed record SchoolResponse(Guid Id, string Name, string OfficialCode, string? Address);

    public sealed class Handler(SchoolFinder finder) : IRequestHandler<Query, Result<SchoolResponse>>
    {
        public async Task<Result<SchoolResponse>> Handle(Query query, CancellationToken ct)
        {
            var found = await finder.FindByIdAsync(query.SchoolId, ct);
            if (found.IsFailure)
            {
                return Result<SchoolResponse>.Failure(found.Error!);
            }

            var school = found.Value;
            return Result<SchoolResponse>.Success(
                new SchoolResponse(school.Id, school.Name, school.OfficialCode, school.Address));
        }
    }
}
