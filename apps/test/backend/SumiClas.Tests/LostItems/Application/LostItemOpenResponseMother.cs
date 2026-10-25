namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemOpenResponseMother
{
    public static LostItemOpenResponse With(int count) => new(count);
}
