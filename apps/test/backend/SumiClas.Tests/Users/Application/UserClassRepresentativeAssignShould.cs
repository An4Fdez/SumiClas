namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassRepresentativeAssignShould : UsersUnitTestCase
{
    private readonly UserClassRepresentativeAssign.Assign _useCase;

    public UserClassRepresentativeAssignShould()
    {
        this._useCase = new UserClassRepresentativeAssign.Assign(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task make_the_target_a_class_representative_when_the_current_user_is_one()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User target = UserMother.Participant(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, target);
        this.ShouldFind(target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.IsSuccess.Should().BeTrue();
        target.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(classId));
    }

    [Test]
    public async Task keep_the_roles_of_the_target_in_other_classes()
    {
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User target = UserMother.With(classRoles: [ClassRoleAssignmentMother.Participant(otherClassId)]);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, target);
        this.ShouldFind(target);

        await this._useCase.ExecuteAsync(UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        target.ClassRoles.Should().BeEquivalentTo(
            [ClassRoleAssignmentMother.Participant(otherClassId), ClassRoleAssignmentMother.Representative(classId)]);
    }

    [Test]
    public async Task make_the_target_a_class_representative_when_the_class_has_two_representatives()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User other = UserMother.Representative(classId);
        User target = UserMother.Random();
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, other, target);
        this.ShouldFind(target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.IsSuccess.Should().BeTrue();
        target.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(classId));
    }

    [Test]
    public async Task return_max_class_representatives_reached_and_leave_the_target_untouched_when_there_are_three()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User second = UserMother.Representative(classId);
        User third = UserMother.Representative(classId);
        User target = UserMother.Participant(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, second, third, target);
        this.ShouldFind(target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.Error.Should().Be(UserErrors.MaxClassRepresentativesReached);
        target.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Participant(classId));
    }

    [Test]
    public async Task not_count_the_representatives_of_other_classes_towards_the_limit()
    {
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User target = UserMother.Random();
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(
            acting,
            UserMother.Representative(otherClassId),
            UserMother.Representative(otherClassId),
            UserMother.Representative(otherClassId),
            target);
        this.ShouldFind(target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task succeed_and_keep_the_role_when_the_target_is_already_a_representative_and_the_class_is_full()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User second = UserMother.Representative(classId);
        User target = UserMother.Representative(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, second, target);
        this.ShouldFind(target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.IsSuccess.Should().BeTrue();
        target.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(classId));
    }

    [Test]
    public async Task leave_the_roles_unchanged_the_second_time_when_it_runs_twice()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        User target = UserMother.Random();
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, target);
        this.ShouldFind(target);

        Result first = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));
        Result second = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        target.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Representative(classId));
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_class_not_found_without_reading_users_when_the_class_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.ShouldNotHaveSearched();
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_school_year_archived_before_checking_the_acting_user_when_both_fail()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        User acting = UserMother.Participant(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), classId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_school_year_not_found_without_reading_users_when_the_school_year_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), classId));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_assign_forbidden_and_leave_the_target_untouched_when_the_current_user_is_a_participant()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Participant(classId);
        User target = UserMother.Participant(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.Error.Should().Be(UserErrors.AssignForbidden);
        target.ClassRoles.Should().Equal(ClassRoleAssignmentMother.Participant(classId));
    }

    [Test]
    public async Task return_assign_forbidden_when_the_current_user_is_a_representative_of_another_class_only()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(GuidMother.Random());
        User target = UserMother.Random();
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, classId));

        result.Error.Should().Be(UserErrors.AssignForbidden);
        target.ClassRoles.Should().BeEmpty();
    }

    [Test]
    public async Task return_assign_forbidden_without_fetching_the_target_when_the_current_user_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchNoUsers();

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), classId));

        result.Error.Should().Be(UserErrors.AssignForbidden);
        this.ShouldNotHaveFetched();
    }

    [Test]
    public async Task return_assign_forbidden_before_user_not_found_when_both_fail()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Participant(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), classId));

        result.Error.Should().Be(UserErrors.AssignForbidden);
    }

    [Test]
    public async Task return_user_not_found_when_the_target_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        User acting = UserMother.Representative(classId);
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchUsers(acting);
        this.ShouldSearchUsers(acting);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(GuidMother.Random(), classId));

        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Test]
    public async Task return_class_id_required_and_leave_the_target_untouched_when_the_class_id_is_empty()
    {
        User acting = UserMother.Representative(Guid.Empty);
        User target = UserMother.Random();
        this.ShouldResolveCurrentUser(acting.Id);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(Guid.Empty, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldSearchUsers(acting, target);
        this.ShouldFind(target);

        Result result = await this._useCase.ExecuteAsync(
            UserClassRepresentativeAssignCommandMother.With(target.Id, Guid.Empty));

        result.Error.Should().Be(UserErrors.ClassIdRequired);
        target.ClassRoles.Should().BeEmpty();
    }
}
