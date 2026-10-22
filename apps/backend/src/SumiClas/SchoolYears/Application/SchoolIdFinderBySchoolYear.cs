using MediatR;
using SumiClas.SchoolYears.Domain;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.SchoolYears.Application;

public static class SchoolIdFinderBySchoolYear
{
    /// <summary>The school a school year belongs to. Other use cases ask it to know the school of a class.</summary>
    public sealed record Query(Guid SchoolYearId) : IQuery<Guid>;

    public sealed class Finder(SchoolYearFinder finder) : IUseCase
    {
        public async Task<Guid> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            SchoolYear schoolYear = await finder.FindByIdAsync(query.SchoolYearId, ct);
            return schoolYear.SchoolId;
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Guid>
    {
        public Task<Guid> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
