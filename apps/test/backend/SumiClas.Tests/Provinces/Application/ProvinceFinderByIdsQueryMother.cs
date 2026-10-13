namespace SumiClas.Tests.Provinces.Application;

using SumiClas.Provinces.Application;

public static class ProvinceFinderByIdsQueryMother
{
    public static ProvinceFinderByIds.Query WithIds(params Guid[] provinceIds) => new(provinceIds);
}
