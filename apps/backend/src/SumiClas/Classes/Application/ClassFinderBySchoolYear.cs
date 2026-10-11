using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Classes.Domain;

namespace SumiClas.Classes.Application;

public static class ClassFinderBySchoolYear
{
    public sealed record Query(Guid SchoolYearId) : IQuery<IReadOnlyCollection<ClassResponse>>;

    public sealed class Finder(IClassRepository repository) : IUseCase
    {
        public async Task<IReadOnlyCollection<ClassResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var classes = await repository.SearchAsync(new ClassSpecification(schoolYearId: query.SchoolYearId), ct);
            return classes
                .Select(schoolClass => new ClassResponse(schoolClass.Id, schoolClass.SchoolYearId, schoolClass.Name))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<ClassResponse>>
    {
        public Task<IReadOnlyCollection<ClassResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
