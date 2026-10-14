namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using SumiClas.Users.Domain.Services;

[TestFixture]
public class UserFinderByIdShould : UsersUnitTestCase
{
    private readonly UserFinderById.Finder _useCase;

    public UserFinderByIdShould()
    {
        this._useCase = new UserFinderById.Finder(new UserFinder(this.Repository.Object));
    }

    [Test]
    public async Task return_only_the_id_and_the_name_of_the_user()
    {
        string fullName = StringMother.Random("name");
        User user = UserMother.WithName(fullName);
        this.ShouldFind(user);

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdQueryMother.WithUserId(user.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UserResponseMother.Create(user.Id, fullName));
    }

    [Test]
    public async Task return_not_found_when_the_user_does_not_exist()
    {
        this.ShouldFetchNoUsers();

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdQueryMother.WithUserId(GuidMother.Random()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Test]
    public async Task return_not_found_for_an_id_other_than_the_one_that_exists()
    {
        this.ShouldFetchUsers(UserMother.Random());

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdQueryMother.WithUserId(GuidMother.Random()),
            CancellationToken.None);

        result.Error.Should().Be(UserErrors.NotFound);
    }
}
