namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using SumiClas.Users.Domain.Services;

[TestFixture]
public class UserProfileFinderCurrentShould : UsersUnitTestCase
{
    private readonly UserProfileFinderCurrent.Finder _useCase;

    public UserProfileFinderCurrentShould()
    {
        this._useCase = new UserProfileFinderCurrent.Finder(
            new UserFinder(this.Repository.Object),
            this.UserProvider.Object);
    }

    [Test]
    public async Task propagate_the_failure_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Result<UserProfileResponse> result = await this._useCase.ExecuteAsync(
            new UserProfileFinderCurrent.Query());

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_not_found_when_the_user_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFetchNoUsers();

        Result<UserProfileResponse> result = await this._useCase.ExecuteAsync(
            new UserProfileFinderCurrent.Query());

        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Test]
    public async Task return_the_profile_with_the_id_the_name_the_email_and_the_class_roles()
    {
        Guid classId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        string email = StringMother.Random("user") + "@example.com";
        User user = UserMother.With(
            fullName: fullName,
            email: EmailMother.Create(email),
            classRoles: [ClassRoleAssignmentMother.Representative(classId)]);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldFind(user);

        Result<UserProfileResponse> result = await this._useCase.ExecuteAsync(
            new UserProfileFinderCurrent.Query());

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.FullName.Should().Be(fullName);
        result.Value.Email.Should().Be(email);
        result.Value.ClassRoles.Select(role => (role.ClassId, role.Role))
            .Should().Equal((classId, "ClassRepresentative"));
    }
}
