using MediatR;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolIdFinderBySchoolYear
{
    /// <summary>The school a school year belongs to. Other use cases ask it to know the school of a class.</summary>
    public sealed record Query(Guid SchoolYearId) : IQuery<Result<Guid>>;

    public sealed class Finder(SchoolYearFinder finder) : IUseCase
    {
        public async Task<Result<Guid>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var found = await finder.FindByIdAsync(query.SchoolYearId, ct);
            return found.IsFailure
                ? Result<Guid>.Failure(found.Error!)
                : Result<Guid>.Success(found.Value.SchoolId);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<Guid>>
    {
        public Task<Result<Guid>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
