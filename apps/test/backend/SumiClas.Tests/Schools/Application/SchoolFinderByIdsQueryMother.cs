namespace SumiClas.Tests.Schools.Application;

using SumiClas.Schools.Application;
using SumiClas.Tests.Shared;

public static class SchoolFinderByIdsQueryMother
{
    public static SchoolFinderByIds.Query With(IReadOnlyCollection<Guid>? schoolIds = null)
        => new(schoolIds ?? [GuidMother.Random()]);

    public static SchoolFinderByIds.Query WithIds(params Guid[] schoolIds) => With(schoolIds);
}
