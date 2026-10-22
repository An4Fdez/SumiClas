namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassAccessEnsureShould : UsersUnitTestCase
{
    private readonly UserClassAccessEnsure.Ensure _useCase;

    public UserClassAccessEnsureShould()
    {
        this._useCase = new UserClassAccessEnsure.Ensure(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task allow_a_representative_of_the_class_without_asking_for_enrollments()
    {
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        this.ShouldSearchUsers(representative);

        await this.Ask(representative.Id, classId);

        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task allow_a_guardian_of_a_student_with_a_confirmed_enrollment_in_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        User guardian = UserMother.Random();
        this.ShouldSearchUsers(guardian);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, guardian.Id));

        Func<Task> act = () => this.Ask(guardian.Id, classId);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task deny_a_guardian_whose_student_has_only_a_suggested_enrollment()
    {
        Guid classId = GuidMother.Random();
        Guid confirmedStudentId = GuidMother.Random();
        Guid suggestedStudentId = GuidMother.Random();
        User guardian = UserMother.Random();
        this.ShouldSearchUsers(guardian);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, confirmedStudentId, EnrollmentResponseMother.CONFIRMED),
            EnrollmentResponseMother.OfStatus(classId, suggestedStudentId, EnrollmentResponseMother.SUGGESTED));
        this.ShouldFindGuardians((confirmedStudentId, GuidMother.Random()), (suggestedStudentId, guardian.Id));

        Func<Task> act = () => this.Ask(guardian.Id, classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
    }

    [Test]
    public async Task deny_a_guardian_of_a_student_enrolled_in_another_class()
    {
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        User guardian = UserMother.Random();
        this.ShouldSearchUsers(guardian);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindEnrollments(
            otherClassId,
            EnrollmentResponseMother.OfStatus(otherClassId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, guardian.Id));

        Func<Task> act = () => this.Ask(guardian.Id, classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
    }

    [Test]
    public async Task deny_a_representative_of_another_class()
    {
        Guid classId = GuidMother.Random();
        User otherClassRepresentative = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(otherClassRepresentative);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindGuardians();

        Func<Task> act = () => this.Ask(otherClassRepresentative.Id, classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
    }

    [Test]
    public async Task deny_another_user_even_when_the_class_has_a_representative()
    {
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        this.ShouldSearchUsers(representative);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindGuardians();

        Func<Task> act = () => this.Ask(GuidMother.Random(), classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
    }

    [Test]
    public async Task deny_a_user_who_is_not_a_guardian_of_any_confirmed_student()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        this.ShouldSearchNoUsers();
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, GuidMother.Random()));

        Func<Task> act = () => this.Ask(GuidMother.Random(), classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
    }

    private Task<Unit> Ask(Guid userId, Guid classId)
        => this._useCase.ExecuteAsync(UserClassAccessEnsureQueryMother.With(userId, classId));
}
