namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserFinderByIdsShould : UsersUnitTestCase
{
    private readonly UserFinderByIds.Finder _useCase;

    public UserFinderByIdsShould()
    {
        this._useCase = new UserFinderByIds.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_only_the_id_and_the_name_of_each_requested_user()
    {
        string firstName = StringMother.Random("first");
        string secondName = StringMother.Random("second");
        User first = UserMother.WithName(firstName);
        User second = UserMother.WithName(secondName);
        this.ShouldSearchUsers(first, second);

        IReadOnlyCollection<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdsQueryMother.WithUserIds(first.Id, second.Id),
            CancellationToken.None);

        result.Should().BeEquivalentTo(
            [UserResponseMother.Create(first.Id, firstName), UserResponseMother.Create(second.Id, secondName)]);
    }

    [Test]
    public async Task not_return_the_users_that_were_not_requested()
    {
        User requested = UserMother.Random();
        this.ShouldSearchUsers(requested, UserMother.Random(), UserMother.Random());

        IReadOnlyCollection<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdsQueryMother.WithUserIds(requested.Id),
            CancellationToken.None);

        result.Select(user => user.Id).Should().Equal(requested.Id);
    }

    [Test]
    public async Task skip_the_ids_of_users_that_do_not_exist()
    {
        User existing = UserMother.Random();
        this.ShouldSearchUsers(existing);

        IReadOnlyCollection<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdsQueryMother.WithUserIds(GuidMother.Random(), existing.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Select(user => user.Id).Should().Equal(existing.Id);
    }

    [Test]
    public async Task return_an_empty_collection_when_no_requested_user_exists()
    {
        this.ShouldSearchUsers(UserMother.Random());

        IReadOnlyCollection<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdsQueryMother.WithUserIds(GuidMother.Random()),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_an_empty_collection_when_no_ids_are_requested_even_if_users_exist()
    {
        this.ShouldSearchUsers(UserMother.Random(), UserMother.Random());

        IReadOnlyCollection<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdsQueryMother.WithUserIds(),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_a_user_requested_twice_only_once()
    {
        User user = UserMother.Random();
        this.ShouldSearchUsers(user);

        IReadOnlyCollection<UserResponse> result = await this._useCase.ExecuteAsync(
            UserFinderByIdsQueryMother.WithUserIds(user.Id, user.Id),
            CancellationToken.None);

        result.Select(response => response.Id).Should().Equal(user.Id);
    }
}
