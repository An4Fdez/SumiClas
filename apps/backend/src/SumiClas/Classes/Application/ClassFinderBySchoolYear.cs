using MediatR;
using SumiClas.Classes.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.Classes.Application;

public static class ClassFinderBySchoolYear
{
    public sealed record Query(Guid SchoolYearId) : IQuery<IReadOnlyCollection<ClassResponse>>;

    public sealed class Handler(ClassesBySchoolYearFinder finder) : IRequestHandler<Query, IReadOnlyCollection<ClassResponse>>
    {
        public async Task<IReadOnlyCollection<ClassResponse>> Handle(Query query, CancellationToken ct)
        {
            var classes = await finder.FindAsync(query.SchoolYearId, ct);
            return classes
                .Select(schoolClass => new ClassResponse(schoolClass.Id, schoolClass.SchoolYearId, schoolClass.Name))
                .ToList();
        }
    }
}
