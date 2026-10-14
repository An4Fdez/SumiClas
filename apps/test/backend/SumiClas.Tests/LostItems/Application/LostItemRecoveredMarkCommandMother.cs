namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemRecoveredMarkCommandMother
{
    public static LostItemRecoveredMark.Command With(Guid lostItemId) => new(lostItemId);
}
