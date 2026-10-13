namespace SumiClas.Tests.Provinces.Application;

using SumiClas.Provinces.Application;

public static class ProvinceResponseMother
{
    public static ProvinceResponse With(Guid id, string name) => new(id, name);
}
