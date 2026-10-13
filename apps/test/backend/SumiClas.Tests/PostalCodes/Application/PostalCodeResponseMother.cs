namespace SumiClas.Tests.PostalCodes.Application;

using SumiClas.PostalCodes.Application;

public static class PostalCodeResponseMother
{
    public static PostalCodeResponse With(Guid id, Guid localityId, string code) => new(id, localityId, code);
}
