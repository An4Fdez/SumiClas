using MediatR;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearEnsureIsActive
{
    public sealed record Query(Guid SchoolYearId) : IQuery<Result>;

    public sealed class Handler(SchoolYearFinder finder) : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(Query query, CancellationToken ct)
        {
            var found = await finder.FindByIdAsync(query.SchoolYearId, ct);
            return found.IsFailure
                ? Result.Failure(found.Error!)
                : found.Value.EnsureIsActive();
        }
    }
}
