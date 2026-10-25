namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain.Events;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassFirstRepresentativeAssignOnClassCreateShould : UsersUnitTestCase
{
    private readonly UserClassFirstRepresentativeAssignOnClassCreate _handler;

    public UserClassFirstRepresentativeAssignOnClassCreateShould()
    {
        this._handler = new UserClassFirstRepresentativeAssignOnClassCreate(this.Repository.Object);
    }

    [Test]
    public async Task make_the_creator_the_representative_of_the_new_class()
    {
        Guid classId = GuidMother.Random();
        User creator = UserMother.Random();
        this.ShouldFetchUsers(creator);
        this.ShouldSearchNoUsers();

        await this._handler.Handle(this.ClassCreatedBy(creator.Id, classId));

        creator.ClassRoles.Should().ContainSingle()
            .Which.Should().Be(ClassRoleAssignmentMother.Representative(classId));
    }

    [Test]
    public async Task throw_not_found_when_the_creator_does_not_exist()
    {
        this.ShouldFetchNoUsers();

        Func<Task> act = () => this._handler.Handle(this.ClassCreatedBy(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
    }

    [Test]
    public async Task throw_already_exists_and_assign_nothing_when_the_class_already_has_a_representative()
    {
        Guid classId = GuidMother.Random();
        User creator = UserMother.Random();
        this.ShouldFetchUsers(creator);
        this.ShouldSearchUsers(UserMother.Representative(classId));

        Func<Task> act = () => this._handler.Handle(this.ClassCreatedBy(creator.Id, classId));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeAlreadyExists>();
        creator.ClassRoles.Should().BeEmpty();
    }

    private ClassCreatedDomainEvent ClassCreatedBy(Guid creatorId, Guid classId)
        => new(classId, GuidMother.Random(), StringMother.Random("class"), creatorId, this.Clock.Object);
}
