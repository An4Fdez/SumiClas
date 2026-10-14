namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserIsClassRepresentativeShould : UsersUnitTestCase
{
    private readonly UserIsClassRepresentative.Check _useCase;

    public UserIsClassRepresentativeShould()
    {
        this._useCase = new UserIsClassRepresentative.Check(this.Repository.Object);
    }

    [Test]
    public async Task deny_a_user_without_roles()
    {
        User user = UserMother.Random();
        this.ShouldSearchUsers(user);

        bool result = await this.Ask(user.Id, GuidMother.Random());

        result.Should().BeFalse();
    }

    [Test]
    public async Task deny_a_user_that_is_participant_in_the_class_and_representative_in_another()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.With(classRoles:
        [
            ClassRoleAssignmentMother.Participant(classId),
            ClassRoleAssignmentMother.Representative(GuidMother.Random()),
        ]);
        this.ShouldSearchUsers(user);

        bool result = await this.Ask(user.Id, classId);

        result.Should().BeFalse();
    }

    [Test]
    public async Task confirm_a_user_that_is_representative_in_the_class_and_participant_in_another()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.With(classRoles:
        [
            ClassRoleAssignmentMother.Participant(GuidMother.Random()),
            ClassRoleAssignmentMother.Representative(classId),
        ]);
        this.ShouldSearchUsers(user);

        bool result = await this.Ask(user.Id, classId);

        result.Should().BeTrue();
    }

    [Test]
    public async Task deny_another_user_even_if_that_other_user_represents_the_class()
    {
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        User participant = UserMother.Participant(classId);
        this.ShouldSearchUsers(representative, participant);

        bool result = await this.Ask(participant.Id, classId);

        result.Should().BeFalse();
    }

    private Task<bool> Ask(Guid userId, Guid classId)
        => this._useCase.ExecuteAsync(
            UserIsClassRepresentativeQueryMother.With(userId, classId),
            CancellationToken.None);
}
