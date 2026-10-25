namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using MediatR;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserAnyClassRepresentativeEnsureShould : UsersUnitTestCase
{
    private readonly UserAnyClassRepresentativeEnsure.Ensure _useCase;

    public UserAnyClassRepresentativeEnsureShould()
    {
        this._useCase = new UserAnyClassRepresentativeEnsure.Ensure(this.Repository.Object);
    }

    [Test]
    public async Task allow_a_representative_of_one_of_the_classes()
    {
        Guid representedClass = GuidMother.Random();
        User representative = UserMother.Representative(representedClass);
        this.ShouldSearchUsers(representative);

        Func<Task> act = () => this.Ask(representative.Id, GuidMother.Random(), representedClass);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task deny_a_participant_of_the_classes()
    {
        Guid classId = GuidMother.Random();
        User participant = UserMother.Participant(classId);
        this.ShouldSearchUsers(participant);

        Func<Task> act = () => this.Ask(participant.Id, classId);

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    [Test]
    public async Task deny_a_representative_of_none_of_the_classes()
    {
        User representative = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(representative);

        Func<Task> act = () => this.Ask(representative.Id, GuidMother.Random(), GuidMother.Random());

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    [Test]
    public async Task deny_another_user_even_when_a_class_has_a_representative()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchUsers(UserMother.Representative(classId));

        Func<Task> act = () => this.Ask(GuidMother.Random(), classId);

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    [Test]
    public async Task deny_a_representative_when_there_are_no_classes()
    {
        User representative = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(representative);

        Func<Task> act = () => this.Ask(representative.Id);

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
    }

    private Task<Unit> Ask(Guid userId, params Guid[] classIds)
        => this._useCase.ExecuteAsync(UserAnyClassRepresentativeEnsureQueryMother.With(userId, classIds));
}
