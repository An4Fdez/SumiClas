namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserSchoolPublishEnsureShould
{
    private readonly UserSchoolPublishEnsure.Ensure _useCase = new();

    [Test]
    public async Task deny_publishing_to_the_school_to_any_user_until_the_admin_and_teacher_roles_exist()
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            UserSchoolPublishEnsureQueryMother.With(GuidMother.Random(), GuidMother.Random()),
            CancellationToken.None);

        (await act.Should().ThrowAsync<UserExceptions.SchoolPublishForbidden>())
            .Which.Code.Should().Be("User.SchoolPublishForbidden");
    }
}
