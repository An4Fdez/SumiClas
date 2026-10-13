namespace SumiClas.Tests.Schools.Application;

using SumiClas.Schools.Application;
using SumiClas.Tests.Shared;

public static class SchoolResponseMother
{
    public static SchoolResponse WithId(Guid id)
        => new(
            id,
            StringMother.Random("school"),
            StringMother.Random("code"),
            GuidMother.Random(),
            StringMother.Random("province"),
            GuidMother.Random(),
            StringMother.Random("locality"),
            null,
            null,
            null,
            null);
}
