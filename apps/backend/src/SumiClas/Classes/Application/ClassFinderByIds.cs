using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Classes.Domain;

namespace SumiClas.Classes.Application;

public static class ClassFinderByIds
{
    /// <summary>Returns only the classes that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> ClassIds) : IQuery<IReadOnlyCollection<ClassResponse>>;

    public sealed class Finder(IClassRepository classRepository) : IUseCase
    {
        public async Task<IReadOnlyCollection<ClassResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var classes = await classRepository.SearchAsync(new ClassSpecification(ids: query.ClassIds), ct);
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
