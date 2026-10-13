namespace SumiClas.Tests.Localities.Application;

using SumiClas.Localities.Application;

public static class LocalityResponseMother
{
    public static LocalityResponse With(Guid id, Guid provinceId, string name) => new(id, provinceId, name);
}
