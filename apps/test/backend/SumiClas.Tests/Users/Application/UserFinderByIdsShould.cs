namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserFinderByIdsShould : UsersUnitTestCase
{
    private readonly UserFinderByIds.Handler _handler;

    public UserFinderByIdsShould()
    {
        this._handler = new UserFinderByIds.Handler(new UserFinderByIds.Finder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        string fullName = StringMother.Random("name");
        User user = UserMother.WithName(fullName);
        this.ShouldReturn(user);

        IReadOnlyCollection<UserResponse> result = await this._handler.Handle(
            UserFinderByIdsQueryMother.WithUserIds(user.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Should().Equal(UserResponseMother.Create(user.Id, fullName));
    }
}
