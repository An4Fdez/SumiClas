namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassRepresentativeEnsureShould : UsersUnitTestCase
{
    private readonly UserClassRepresentativeEnsure.Ensure _useCase;

    public UserClassRepresentativeEnsureShould()
    {
        this._useCase = new UserClassRepresentativeEnsure.Ensure(this.Repository.Object);
    }

    [Test]
    public async Task allow_a_representative_of_the_class()
    {
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        this.ShouldSearchUsers(representative);

        Result result = await this.Ask(representative.Id, classId);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task deny_a_participant_of_the_class()
    {
        Guid classId = GuidMother.Random();
        User participant = UserMother.Participant(classId);
        this.ShouldSearchUsers(participant);

        Result result = await this.Ask(participant.Id, classId);

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
    }

    [Test]
    public async Task deny_a_representative_of_another_class()
    {
        User representative = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(representative);

        Result result = await this.Ask(representative.Id, GuidMother.Random());

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
    }

    [Test]
    public async Task deny_another_user_even_when_the_class_has_a_representative()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchUsers(UserMother.Representative(classId));

        Result result = await this.Ask(GuidMother.Random(), classId);

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
    }

    private Task<Result> Ask(Guid userId, Guid classId)
        => this._useCase.ExecuteAsync(UserClassRepresentativeEnsureQueryMother.With(userId, classId));
}
