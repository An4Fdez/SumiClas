namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserNameChangeShould : UsersUnitTestCase
{
    private readonly UserNameChange.Change _useCase;

    public UserNameChangeShould()
    {
        this._useCase = new UserNameChange.Change(this.Repository.Object, this.UserProvider.Object);
    }

    [Test]
    public async Task rename_only_the_current_user_trimming_the_name()
    {
        User current = UserMother.Random();
        User other = UserMother.Random();
        string otherName = other.FullName;
        string newName = StringMother.Random("name");
        this.ShouldFetchUsers(current, other);
        this.ShouldResolveCurrentUser(current.Id);

        await this._useCase.ExecuteAsync(UserNameChangeCommandMother.With("  " + newName + " "));

        current.FullName.Should().Be(newName);
        other.FullName.Should().Be(otherName);
    }

    [Test]
    public async Task throw_full_name_required_and_keep_the_name_when_it_is_blank()
    {
        User current = UserMother.Random();
        string previousName = current.FullName;
        this.ShouldFetchUsers(current);
        this.ShouldResolveCurrentUser(current.Id);

        Func<Task> act = () => this._useCase.ExecuteAsync(UserNameChangeCommandMother.With("   "));

        await act.Should().ThrowAsync<UserExceptions.FullNameRequired>();
        current.FullName.Should().Be(previousName);
    }

    [Test]
    public async Task throw_not_found_when_the_current_user_does_not_exist()
    {
        this.ShouldFetchNoUsers();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(UserNameChangeCommandMother.With());

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
    }

    [Test]
    public async Task throw_unauthenticated_without_fetching_the_user_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(UserNameChangeCommandMother.With());

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveFetched();
    }
}
