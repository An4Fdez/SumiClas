using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Classes.Domain;

namespace SumiClas.Classes.Application;

public static class ClassFinderByIds
{
    /// <summary>Returns only the classes that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> ClassIds) : IQuery<IReadOnlyCollection<ClassResponse>>;

    public sealed class Handler(IClassRepository classRepository) : IRequestHandler<Query, IReadOnlyCollection<ClassResponse>>
    {
        public async Task<IReadOnlyCollection<ClassResponse>> Handle(Query query, CancellationToken ct)
        {
            var classes = await classRepository.SearchAsync(new ClassSpecification(ids: query.ClassIds), ct);
            return classes
                .Select(schoolClass => new ClassResponse(schoolClass.Id, schoolClass.SchoolYearId, schoolClass.Name))
                .ToList();
        }
    }
}
