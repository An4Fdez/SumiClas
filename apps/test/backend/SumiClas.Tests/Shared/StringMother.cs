namespace SumiClas.Tests.Shared;

public static class StringMother
{
    public static string Random(string prefix) => prefix + "-" + GuidMother.Random().ToString("N");
}
