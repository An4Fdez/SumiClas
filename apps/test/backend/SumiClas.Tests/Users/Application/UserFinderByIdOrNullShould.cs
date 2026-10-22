namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserFinderByIdOrNullShould : UsersUnitTestCase
{
    private readonly UserFinderByIdOrNull.Finder _useCase;

    public UserFinderByIdOrNullShould()
    {
        this._useCase = new UserFinderByIdOrNull.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_only_the_id_and_the_name_of_the_user()
    {
        string fullName = StringMother.Random("name");
        User user = UserMother.WithName(fullName);
        this.ShouldFetchUsers(user, UserMother.Random());

        UserResponse? result = await this._useCase.ExecuteAsync(
            UserFinderByIdOrNullQueryMother.WithUserId(user.Id),
            CancellationToken.None);

        result.Should().Be(UserResponseMother.Create(user.Id, fullName));
    }

    [Test]
    public async Task return_nothing_for_an_id_that_does_not_exist()
    {
        this.ShouldFetchUsers(UserMother.Random());

        UserResponse? result = await this._useCase.ExecuteAsync(
            UserFinderByIdOrNullQueryMother.WithUserId(GuidMother.Random()),
            CancellationToken.None);

        result.Should().BeNull();
    }
}
