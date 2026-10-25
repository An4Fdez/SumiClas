namespace SumiClas.Tests.Provinces.Domain;

using AwesomeAssertions;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ProvinceShould
{
    [Test]
    public void create_the_province_with_a_trimmed_name_and_its_normalized_form()
    {
        Guid id = GuidMother.Random();
        string name = "  Málaga ";

        Province result = Province.Create(id, name);

        result.Id.Should().Be(id);
        result.Name.Should().Be(name.Trim());
        result.NormalizedName.Should().Be(TextNormalizer.Normalize(name));
    }

    [Test]
    public void throw_id_required_when_created_with_an_empty_id()
    {
        Action act = () => Province.Create(Guid.Empty, StringMother.Random("Province"));

        act.Should().Throw<ProvinceExceptions.IdRequired>();
    }

    [TestCase(null)]
    [TestCase("   ")]
    public void throw_name_required_when_created_with_an_empty_name(string? name)
    {
        Action act = () => Province.Create(GuidMother.Random(), name!);

        act.Should().Throw<ProvinceExceptions.NameRequired>();
    }

    [Test]
    public void report_the_id_before_the_name_when_both_are_invalid()
    {
        Action act = () => Province.Create(Guid.Empty, string.Empty);

        act.Should().Throw<ProvinceExceptions.IdRequired>();
    }
}
