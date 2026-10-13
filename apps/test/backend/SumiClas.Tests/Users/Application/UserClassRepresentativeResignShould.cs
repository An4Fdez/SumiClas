namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes;
using SumiClas.Tests.SchoolYears;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassRepresentativeResignShould : UsersUnitTestCase
{
    private readonly UserClassRepresentativeResign.Resign _useCase;

    public UserClassRepresentativeResignShould()
    {
        this._useCase = new UserClassRepresentativeResign.Resign(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task turn_the_current_user_into_a_participant_when_another_representative_remains()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.Representative(classId);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user, UserMother.Representative(classId));
        this.ShouldFind(user);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        user.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Participant(classId));
    }

    [Test]
    public async Task keep_the_roles_of_the_user_in_other_classes()
    {
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        User user = UserMother.With(classRoles:
        [
            ClassRoleAssignmentMother.Representative(classId),
            ClassRoleAssignmentMother.Representative(otherClassId),
        ]);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user, UserMother.Representative(classId));
        this.ShouldFind(user);

        await this._useCase.ExecuteAsync(UserClassRepresentativeResignCommandMother.WithClassId(classId));

        user.ClassRoles.Should().BeEquivalentTo(
            [ClassRoleAssignmentMother.Representative(otherClassId), ClassRoleAssignmentMother.Participant(classId)]);
    }

    [Test]
    public async Task return_last_representative_cannot_resign_and_keep_the_role_when_the_user_is_the_only_one()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.Representative(classId);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user);
        this.ShouldFind(user);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.LastClassRepresentativeCannotResign);
        user.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(classId));
    }

    [Test]
    public async Task not_count_the_representatives_of_other_classes_when_checking_the_last_one()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.Representative(classId);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user, UserMother.Representative(GuidMother.Random()));
        this.ShouldFind(user);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.LastClassRepresentativeCannotResign);
    }

    [Test]
    public async Task return_not_class_representative_the_second_time_when_it_runs_twice()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.Representative(classId);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user, UserMother.Representative(classId));
        this.ShouldFind(user);

        Result first = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));
        Result second = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        first.IsSuccess.Should().BeTrue();
        second.Error.Should().Be(UserErrors.NotClassRepresentative);
        user.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Participant(classId));
    }

    [Test]
    public async Task return_not_class_representative_when_the_user_is_a_participant_and_another_user_is_the_only_one()
    {
        Guid classId = GuidMother.Random();
        User user = UserMother.Participant(classId);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user, UserMother.Representative(classId));
        this.ShouldFind(user);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.NotClassRepresentative);
        user.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Participant(classId));
    }

    [Test]
    public async Task return_no_role_assigned_when_the_user_only_has_a_role_in_another_class()
    {
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        User user = UserMother.Representative(otherClassId);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(classId, GuidMother.Random());
        this.ShouldReturn(user);
        this.ShouldFind(user);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.NoRoleAssignedInClass);
        user.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(otherClassId));
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();
        this.QueryBus.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(GuidMother.Random()));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_class_not_found_without_reading_users_when_the_class_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.QueryBus.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(GuidMother.Random()));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.ShouldNotHaveSearched();
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_school_year_archived_before_looking_for_the_user_when_both_fail()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_school_year_not_found_without_reading_users_when_the_school_year_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_user_not_found_without_searching_when_the_current_user_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldResolveActiveClass(classId, GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.NotFound);
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_class_id_required_when_the_class_id_is_empty()
    {
        User user = UserMother.Representative(Guid.Empty);
        this.ShouldResolveCurrentUser(user.Id);
        this.ShouldResolveActiveClass(Guid.Empty, GuidMother.Random());
        this.ShouldReturn(user, UserMother.Representative(Guid.Empty));
        this.ShouldFind(user);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeResignCommandMother.WithClassId(Guid.Empty));

        result.Error.Should().Be(UserErrors.ClassIdRequired);
        user.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(Guid.Empty));
    }
}
