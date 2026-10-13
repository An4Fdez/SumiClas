namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassMemberFinderByIdsShould : UsersUnitTestCase
{
    private readonly UserClassMemberFinderByIds.Handler _handler;

    public UserClassMemberFinderByIdsShould()
    {
        this._handler = new UserClassMemberFinderByIds.Handler(
            new UserClassMemberFinderByIds.Finder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        Guid classId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        User user = UserMother.Representative(classId, fullName);
        this.ShouldReturn(user);

        IReadOnlyCollection<UserClassMemberResponse> result = await this._handler.Handle(
            UserClassMemberFinderByIdsQueryMother.WithUserIds(classId, user.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Should().Equal(UserClassMemberResponseMother.Representative(user.Id, fullName));
    }
}
