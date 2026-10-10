using MediatR;
using SumiClas.Schools.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Application;

public static class SchoolFinderById
{
    public sealed record Query(Guid SchoolId) : IQuery<Result<SchoolResponse>>;

    public sealed class Handler(SchoolFinder finder, IQueryBus queryBus) : IRequestHandler<Query, Result<SchoolResponse>>
    {
        public async Task<Result<SchoolResponse>> Handle(Query query, CancellationToken ct)
        {
            var found = await finder.FindByIdAsync(query.SchoolId, ct);
            if (found.IsFailure)
            {
                return Result<SchoolResponse>.Failure(found.Error!);
            }

            var responses = await SchoolResponseFactory.CreateAsync(queryBus, [found.Value], ct);
            return Result<SchoolResponse>.Success(responses.Single());
        }
    }
}
