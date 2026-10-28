namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemUpdateCommandMother
{
    public static LostItemUpdate.Command With(Guid id, string description) => new(id, description);
}
