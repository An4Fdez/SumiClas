namespace SumiClas.Tests.Schools.Application;

using SumiClas.Schools.Application;

public static class SchoolFinderByLocalityQueryMother
{
    public static SchoolFinderByLocality.Query With(Guid? localityId, string? name = null) => new(localityId, name);

    public static SchoolFinderByLocality.Query InLocality(Guid localityId) => With(localityId);

    public static SchoolFinderByLocality.Query WithoutLocality(Guid? localityId) => With(localityId);
}
