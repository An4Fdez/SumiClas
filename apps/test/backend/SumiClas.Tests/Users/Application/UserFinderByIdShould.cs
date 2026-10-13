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
    private readonly UserFinderById.Handler _handler;

    public UserFinderByIdShould()
    {
        this._handler = new UserFinderById.Handler(new UserFinderById.Finder(new UserFinder(this.Repository.Object)));
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        string fullName = StringMother.Random("name");
        User user = UserMother.WithName(fullName);
        this.ShouldFind(user);

        Result<UserResponse> result = await this._handler.Handle(
            UserFinderByIdQueryMother.WithUserId(user.Id),
            CancellationToken.None);

        result.Value.Should().Be(UserResponseMother.Create(user.Id, fullName));
    }
}
