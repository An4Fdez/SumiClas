namespace SumiClas.Tests.Localities.Application;

using SumiClas.Localities.Application;

public static class LocalityFinderByIdsQueryMother
{
    public static LocalityFinderByIds.Query WithIds(params Guid[] localityIds) => new(localityIds);
}
