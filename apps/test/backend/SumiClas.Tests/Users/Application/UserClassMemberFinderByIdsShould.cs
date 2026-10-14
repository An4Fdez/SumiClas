namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassMemberFinderByIdsShould : UsersUnitTestCase
{
    private readonly UserClassMemberFinderByIds.Finder _useCase;

    public UserClassMemberFinderByIdsShould()
    {
        this._useCase = new UserClassMemberFinderByIds.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_the_representative_role_of_a_representative_of_the_class()
    {
        Guid classId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        User representative = UserMother.Representative(classId, fullName);
        this.ShouldSearchUsers(representative);

        IReadOnlyCollection<UserClassMemberResponse> result = await this.Ask(classId, representative.Id);

        result.Should().Equal(UserClassMemberResponseMother.Representative(representative.Id, fullName));
    }

    [Test]
    public async Task return_the_participant_role_of_a_participant_of_the_class()
    {
        Guid classId = GuidMother.Random();
        User participant = UserMother.Participant(classId);
        this.ShouldSearchUsers(participant);

        IReadOnlyCollection<UserClassMemberResponse> result = await this.Ask(classId, participant.Id);

        result.Should().Equal(UserClassMemberResponseMother.Participant(participant.Id, participant.FullName));
    }

    [Test]
    public async Task return_no_role_for_a_user_without_roles()
    {
        User user = UserMother.Random();
        this.ShouldSearchUsers(user);

        IReadOnlyCollection<UserClassMemberResponse> result = await this.Ask(GuidMother.Random(), user.Id);

        result.Should().Equal(UserClassMemberResponseMother.WithoutRole(user.Id, user.FullName));
    }

    [Test]
    public async Task return_no_role_for_a_user_whose_only_role_is_in_another_class()
    {
        User representativeElsewhere = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(representativeElsewhere);

        IReadOnlyCollection<UserClassMemberResponse> result =
            await this.Ask(GuidMother.Random(), representativeElsewhere.Id);

        result.Should().Equal(
            UserClassMemberResponseMother.WithoutRole(representativeElsewhere.Id, representativeElsewhere.FullName));
    }

    [Test]
    public async Task return_the_role_of_the_asked_class_for_a_user_with_roles_in_several_classes()
    {
        Guid askedClassId = GuidMother.Random();
        User user = UserMother.With(classRoles:
        [
            ClassRoleAssignmentMother.Representative(GuidMother.Random()),
            ClassRoleAssignmentMother.Participant(askedClassId),
            ClassRoleAssignmentMother.Representative(GuidMother.Random()),
        ]);
        this.ShouldSearchUsers(user);

        IReadOnlyCollection<UserClassMemberResponse> result = await this.Ask(askedClassId, user.Id);

        result.Should().Equal(UserClassMemberResponseMother.Participant(user.Id, user.FullName));
    }

    [Test]
    public async Task return_each_requested_user_with_their_own_role()
    {
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        User participant = UserMother.Participant(classId);
        User outsider = UserMother.Random();
        this.ShouldSearchUsers(representative, participant, outsider);

        IReadOnlyCollection<UserClassMemberResponse> result =
            await this.Ask(classId, representative.Id, participant.Id, outsider.Id);

        result.Should().BeEquivalentTo(
        [
            UserClassMemberResponseMother.Representative(representative.Id, representative.FullName),
            UserClassMemberResponseMother.Participant(participant.Id, participant.FullName),
            UserClassMemberResponseMother.WithoutRole(outsider.Id, outsider.FullName),
        ]);
    }

    [Test]
    public async Task not_return_the_users_that_were_not_requested()
    {
        Guid classId = GuidMother.Random();
        User requested = UserMother.Participant(classId);
        this.ShouldSearchUsers(requested, UserMother.Participant(classId));

        IReadOnlyCollection<UserClassMemberResponse> result = await this.Ask(classId, requested.Id);

        result.Select(member => member.UserId).Should().Equal(requested.Id);
    }

    [Test]
    public async Task return_an_empty_collection_when_no_ids_are_requested_even_if_users_exist()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchUsers(UserMother.Participant(classId), UserMother.Representative(classId));

        IReadOnlyCollection<UserClassMemberResponse> result = await this.Ask(classId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private Task<IReadOnlyCollection<UserClassMemberResponse>> Ask(Guid classId, params Guid[] userIds)
        => this._useCase.ExecuteAsync(
            UserClassMemberFinderByIdsQueryMother.With(classId, userIds),
            CancellationToken.None);
}
