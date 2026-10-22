namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using MediatR;
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

        Func<Task> act = () => this.Ask(representative.Id, classId);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task deny_a_participant_of_the_class()
    {
        Guid classId = GuidMother.Random();
        User participant = UserMother.Participant(classId);
        this.ShouldSearchUsers(participant);

        Func<Task> act = () => this.Ask(participant.Id, classId);

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    [Test]
    public async Task deny_a_representative_of_another_class()
    {
        User representative = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(representative);

        Func<Task> act = () => this.Ask(representative.Id, GuidMother.Random());

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    [Test]
    public async Task deny_another_user_even_when_the_class_has_a_representative()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchUsers(UserMother.Representative(classId));

        Func<Task> act = () => this.Ask(GuidMother.Random(), classId);

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    private Task<Unit> Ask(Guid userId, Guid classId)
        => this._useCase.ExecuteAsync(UserClassRepresentativeEnsureQueryMother.With(userId, classId));
}
