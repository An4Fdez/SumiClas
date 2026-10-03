using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Classes.Domain;

namespace SumiClas.Classes.Application;

public static class ClassFinderBySchoolYear
{
    public sealed record Query(Guid SchoolYearId) : IQuery<IReadOnlyCollection<ClassResponse>>;

    public sealed class Handler(IClassRepository classRepository) : IRequestHandler<Query, IReadOnlyCollection<ClassResponse>>
    {
        public async Task<IReadOnlyCollection<ClassResponse>> Handle(Query query, CancellationToken ct)
        {
            var classes = await classRepository.SearchAsync(new ClassSpecification(schoolYearId: query.SchoolYearId), ct);
            return classes
                .Select(schoolClass => new ClassResponse(schoolClass.Id, schoolClass.SchoolYearId, schoolClass.Name))
                .ToList();
        }
    }
}
