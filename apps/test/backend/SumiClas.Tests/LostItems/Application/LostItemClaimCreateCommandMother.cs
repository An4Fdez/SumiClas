namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;
using SumiClas.Tests.Shared;

public static class LostItemClaimCreateCommandMother
{
    public static LostItemClaimCreate.Command For(Guid lostItemId)
        => new(GuidMother.Random(), lostItemId, StringMother.Random("detail"));

    public static LostItemClaimCreate.Command With(Guid id, Guid lostItemId, string message)
        => new(id, lostItemId, message);
}
