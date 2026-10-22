namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserIsClassMemberShould : UsersUnitTestCase
{
    private readonly UserIsClassMember.Check _useCase;

    public UserIsClassMemberShould()
    {
        this._useCase = new UserIsClassMember.Check(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task confirm_a_representative_of_the_class_without_asking_for_enrollments()
    {
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        this.ShouldSearchUsers(representative);

        bool result = await this.Ask(representative.Id, classId);

        result.Should().BeTrue();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task confirm_a_guardian_of_a_student_with_a_confirmed_enrollment_in_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        User guardian = UserMother.Random();
        this.ShouldSearchUsers(guardian);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, guardian.Id));

        bool result = await this.Ask(guardian.Id, classId);

        result.Should().BeTrue();
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

        bool result = await this.Ask(guardian.Id, classId);

        result.Should().BeFalse();
    }

    [Test]
    public async Task deny_a_representative_of_another_class()
    {
        Guid classId = GuidMother.Random();
        User otherClassRepresentative = UserMother.Representative(GuidMother.Random());
        this.ShouldSearchUsers(otherClassRepresentative);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindGuardians();

        bool result = await this.Ask(otherClassRepresentative.Id, classId);

        result.Should().BeFalse();
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

        bool result = await this.Ask(GuidMother.Random(), classId);

        result.Should().BeFalse();
    }

    private Task<bool> Ask(Guid userId, Guid classId)
        => this._useCase.ExecuteAsync(UserIsClassMemberQueryMother.With(userId, classId));
}
