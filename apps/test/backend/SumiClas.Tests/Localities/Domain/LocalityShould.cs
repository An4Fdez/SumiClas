namespace SumiClas.Tests.Localities.Domain;

using AwesomeAssertions;
using SumiClas.Localities.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LocalityShould
{
    [Test]
    public void create_the_locality_with_a_trimmed_name_and_its_normalized_form()
    {
        Guid id = GuidMother.Random();
        Guid provinceId = GuidMother.Random();
        string name = "  Alcalá de Henares ";

        Locality result = Locality.Create(id, provinceId, name);

        result.Id.Should().Be(id);
        result.ProvinceId.Should().Be(provinceId);
        result.Name.Should().Be(name.Trim());
        result.NormalizedName.Should().Be(TextNormalizer.Normalize(name));
    }

    [Test]
    public void throw_id_required_when_created_with_an_empty_id()
    {
        Action act = () => Locality.Create(Guid.Empty, GuidMother.Random(), StringMother.Random("Locality"));

        act.Should().Throw<LocalityExceptions.IdRequired>();
    }

    [Test]
    public void throw_province_required_when_created_without_a_province()
    {
        Action act = () => Locality.Create(GuidMother.Random(), Guid.Empty, StringMother.Random("Locality"));

        act.Should().Throw<LocalityExceptions.ProvinceRequired>();
    }

    [TestCase(null)]
    [TestCase("   ")]
    public void throw_name_required_when_created_with_an_empty_name(string? name)
    {
        Action act = () => Locality.Create(GuidMother.Random(), GuidMother.Random(), name!);

        act.Should().Throw<LocalityExceptions.NameRequired>();
    }

    [Test]
    public void report_the_id_first_when_everything_is_invalid()
    {
        Action act = () => Locality.Create(Guid.Empty, Guid.Empty, string.Empty);

        act.Should().Throw<LocalityExceptions.IdRequired>();
    }

    [Test]
    public void report_the_province_before_the_name_when_both_are_invalid()
    {
        Action act = () => Locality.Create(GuidMother.Random(), Guid.Empty, string.Empty);

        act.Should().Throw<LocalityExceptions.ProvinceRequired>();
    }
}
