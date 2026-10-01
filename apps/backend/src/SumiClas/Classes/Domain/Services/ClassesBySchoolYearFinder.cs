
namespace SumiClas.Classes.Domain.Services;

public sealed class ClassesBySchoolYearFinder(IClassRepository classes)
{
    public Task<IReadOnlyCollection<Class>> FindAsync(Guid schoolYearId, CancellationToken ct = default)
        => classes.SearchAsync(new ClassSpecification(schoolYearId: schoolYearId), ct);
}
