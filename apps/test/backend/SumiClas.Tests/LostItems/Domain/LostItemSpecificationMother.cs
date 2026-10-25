namespace SumiClas.Tests.LostItems.Domain;

using SumiClas.LostItems.Domain;

public static class LostItemSpecificationMother
{
    public static LostItemSpecification CreatedFrom(DateTimeOffset moment) => new(createdFrom: moment);
}
