namespace SumiClas.Tests.LostItems.Domain;

using AwesomeAssertions;
using SumiClas.LostItems.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LostItemShould : UnitTestCase
{
    [Test]
    public void throw_school_required_when_a_school_item_has_no_school()
    {
        Func<LostItem> act = () => this.CreateWithScope(ItemScope.School, GuidMother.Random(), null);

        act.Should().Throw<LostItemExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void throw_school_required_when_a_school_item_has_an_empty_school()
    {
        Func<LostItem> act = () => this.CreateWithScope(ItemScope.School, GuidMother.Random(), Guid.Empty);

        act.Should().Throw<LostItemExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void keep_only_the_class_when_the_item_is_of_the_class_scope()
    {
        Guid classId = GuidMother.Random();

        LostItem lostItem = this.CreateWithScope(ItemScope.Class, classId, GuidMother.Random());

        lostItem.ClassId.Should().Be(classId);
        lostItem.SchoolId.Should().BeNull();
    }

    [Test]
    public void keep_only_the_school_when_the_item_is_of_the_school_scope()
    {
        Guid schoolId = GuidMother.Random();

        LostItem lostItem = this.CreateWithScope(ItemScope.School, GuidMother.Random(), schoolId);

        lostItem.SchoolId.Should().Be(schoolId);
        lostItem.ClassId.Should().BeNull();
    }

    private LostItem CreateWithScope(ItemScope scope, Guid? classId, Guid? schoolId)
        => LostItem.Create(
            GuidMother.Random(),
            scope,
            classId,
            schoolId,
            ItemStatus.Lost,
            StringMother.Random("Item"),
            null,
            false,
            GuidMother.Random(),
            this.Clock.Object);
}
