namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserIsClassRepresentativeShould : UsersUnitTestCase
{
    private readonly UserIsClassRepresentative.Handler _handler;

    public UserIsClassRepresentativeShould()
    {
        this._handler = new UserIsClassRepresentative.Handler(
            new UserIsClassRepresentative.Check(this.Repository.Object));
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.Representative(classId);
        this.ShouldReturn(user);

        bool result = await this._handler.Handle(
            UserIsClassRepresentativeQueryMother.With(user.Id, classId),
            CancellationToken.None);

        result.Should().BeTrue();
    }
}
