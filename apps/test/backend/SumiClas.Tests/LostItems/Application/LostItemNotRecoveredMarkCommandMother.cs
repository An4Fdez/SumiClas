namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemNotRecoveredMarkCommandMother
{
    public static LostItemNotRecoveredMark.Command With(Guid lostItemId) => new(lostItemId);
}
