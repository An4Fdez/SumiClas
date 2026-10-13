namespace SumiClas.Tests.PostalCodes.Application;

using SumiClas.PostalCodes.Application;

public static class PostalCodeFinderByIdsQueryMother
{
    public static PostalCodeFinderByIds.Query WithIds(params Guid[] postalCodeIds) => new(postalCodeIds);
}
