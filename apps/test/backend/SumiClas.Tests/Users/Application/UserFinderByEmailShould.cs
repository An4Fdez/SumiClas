namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserFinderByEmailShould : UsersUnitTestCase
{
    private readonly UserFinderByEmail.Finder _useCase;

    public UserFinderByEmailShould()
    {
        this._useCase = new UserFinderByEmail.Finder(this.Repository.Object);
    }

    [Test]
    public async Task find_the_user_ignoring_case_and_spaces_and_return_only_the_id_and_the_name()
    {
        string fullName = StringMother.Random("name");
        string email = StringMother.Random("user") + "@example.com";
        User user = UserMother.With(fullName: fullName, email: EmailMother.Create(email));
        this.ShouldSearchUsers(user);

        UserResponse result = await this._useCase.ExecuteAsync(
            UserFinderByEmailQueryMother.WithEmail("  " + email.ToUpperInvariant() + " "),
            CancellationToken.None);
        result.Should().Be(UserResponseMother.Create(user.Id, fullName));
    }

    [Test]
    public async Task throw_not_found_when_the_email_is_unknown()
    {
        this.ShouldSearchNoUsers();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            UserFinderByEmailQueryMother.WithEmail(StringMother.Random("nobody") + "@example.com"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
    }

    [Test]
    public async Task throw_invalid_email_format_when_the_email_is_malformed()
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            UserFinderByEmailQueryMother.WithEmail("nonsense"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UserExceptions.InvalidEmailFormat>();
    }
}
