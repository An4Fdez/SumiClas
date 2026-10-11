using MediatR;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearEnsureIsActive
{
    public sealed record Query(Guid SchoolYearId) : IQuery<Result>;

    public sealed class EnsureIsActive(SchoolYearFinder finder) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var found = await finder.FindByIdAsync(query.SchoolYearId, ct);
            return found.IsFailure
                ? Result.Failure(found.Error!)
                : found.Value.EnsureIsActive();
        }
    }

    public sealed class Handler(EnsureIsActive ensureIsActive) : IRequestHandler<Query, Result>
    {
        public Task<Result> Handle(Query query, CancellationToken ct)
            => ensureIsActive.ExecuteAsync(query, ct);
    }
}
