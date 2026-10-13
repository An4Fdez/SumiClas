namespace SumiClas.Tests.Localities.Application;

using SumiClas.Localities.Application;

public static class LocalityFinderByProvinceQueryMother
{
    public static LocalityFinderByProvince.Query With(Guid provinceId, string? name = null) => new(provinceId, name);

    public static LocalityFinderByProvince.Query WithProvinceId(Guid provinceId) => With(provinceId);

    public static LocalityFinderByProvince.Query WithName(Guid provinceId, string name) => With(provinceId, name);
}
