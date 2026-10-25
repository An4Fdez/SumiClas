namespace SumiClas.Tests.PostalCodes.Domain;

using AwesomeAssertions;
using SumiClas.PostalCodes.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class PostalCodeShould
{
    private static readonly string ValidCode = new('1', PostalCode.Length);

    [Test]
    public void create_the_postal_code_with_the_code_trimmed()
    {
        Guid id = GuidMother.Random();
        Guid localityId = GuidMother.Random();

        PostalCode result = PostalCode.Create(id, localityId, $" {ValidCode} ");

        result.Id.Should().Be(id);
        result.LocalityId.Should().Be(localityId);
        result.Code.Should().Be(ValidCode);
    }

    [Test]
    public void throw_id_required_when_created_with_an_empty_id()
    {
        Action act = () => PostalCode.Create(Guid.Empty, GuidMother.Random(), ValidCode);

        act.Should().Throw<PostalCodeExceptions.IdRequired>();
    }

    [Test]
    public void throw_locality_required_when_created_without_a_locality()
    {
        Action act = () => PostalCode.Create(GuidMother.Random(), Guid.Empty, ValidCode);

        act.Should().Throw<PostalCodeExceptions.LocalityRequired>();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("1234")]
    [TestCase("123456")]
    [TestCase("12a45")]
    public void throw_code_invalid_when_the_code_is_not_five_digits(string? code)
    {
        Action act = () => PostalCode.Create(GuidMother.Random(), GuidMother.Random(), code!);

        act.Should().Throw<PostalCodeExceptions.CodeInvalid>();
    }

    [Test]
    public void report_the_id_first_when_everything_is_invalid()
    {
        Action act = () => PostalCode.Create(Guid.Empty, Guid.Empty, string.Empty);

        act.Should().Throw<PostalCodeExceptions.IdRequired>();
    }

    [Test]
    public void report_the_locality_before_the_code_when_both_are_invalid()
    {
        Action act = () => PostalCode.Create(GuidMother.Random(), Guid.Empty, string.Empty);

        act.Should().Throw<PostalCodeExceptions.LocalityRequired>();
    }
}
