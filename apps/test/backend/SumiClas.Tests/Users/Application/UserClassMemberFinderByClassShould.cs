namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassMemberFinderByClassShould : UsersUnitTestCase
{
    private readonly UserClassMemberFinderByClass.Finder _useCase;

    public UserClassMemberFinderByClassShould()
    {
        this._useCase = new UserClassMemberFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_guardians_and_representatives_with_the_representatives_first_then_by_name()
    {
        Guid classId = GuidMother.Random();
        Guid zoeStudentId = GuidMother.Random();
        Guid anaStudentId = GuidMother.Random();
        string anaName = StringMother.Random("Ana");
        string representativeName = StringMother.Random("Marta");
        string zoeName = StringMother.Random("zoe");
        User zoe = UserMother.WithName(zoeName);
        User ana = UserMother.WithName(anaName);
        User representative = UserMother.Representative(classId, representativeName);
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(ana.Id);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, zoeStudentId, EnrollmentResponseMother.CONFIRMED),
            EnrollmentResponseMother.OfStatus(classId, anaStudentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((zoeStudentId, zoe.Id), (anaStudentId, ana.Id), (anaStudentId, representative.Id));
        this.ShouldSearchUsers(zoe, ana, representative);

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(member => member.FullName).Should().Equal(representativeName, anaName, zoeName);
        result.Value.Select(member => member.ClassRole)
            .Should().Equal("ClassRepresentative", "Participant", "Participant");
    }

    [Test]
    public async Task return_a_guardian_of_two_children_only_once()
    {
        Guid classId = GuidMother.Random();
        Guid firstStudentId = GuidMother.Random();
        Guid secondStudentId = GuidMother.Random();
        User parent = UserMother.WithName(StringMother.Random("name"));
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(parent.Id);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, firstStudentId, EnrollmentResponseMother.CONFIRMED),
            EnrollmentResponseMother.OfStatus(classId, secondStudentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((firstStudentId, parent.Id), (secondStudentId, parent.Id));
        this.ShouldSearchUsers(parent);

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Value.Select(member => member.UserId).Should().Equal(parent.Id);
    }

    [Test]
    public async Task not_return_the_guardian_of_a_suggested_enrollment_as_a_member()
    {
        Guid classId = GuidMother.Random();
        Guid confirmedStudentId = GuidMother.Random();
        Guid suggestedStudentId = GuidMother.Random();
        User confirmedParent = UserMother.WithName(StringMother.Random("name"));
        User suggestedParent = UserMother.WithName(StringMother.Random("name"));
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(confirmedParent.Id);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, confirmedStudentId, EnrollmentResponseMother.CONFIRMED),
            EnrollmentResponseMother.OfStatus(classId, suggestedStudentId, EnrollmentResponseMother.SUGGESTED));
        this.ShouldFindGuardians((confirmedStudentId, confirmedParent.Id), (suggestedStudentId, suggestedParent.Id));
        this.ShouldSearchUsers(confirmedParent, suggestedParent);

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Value.Select(member => member.UserId).Should().Equal(confirmedParent.Id);
    }

    [Test]
    public async Task include_a_representative_without_children_and_list_them_first()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        User parent = UserMother.WithName(StringMother.Random("name"));
        User representative = UserMother.Representative(classId);
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(representative.Id);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, parent.Id));
        this.ShouldSearchUsers(parent, representative);

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.IsSuccess.Should().BeTrue();
        result.Value.First().UserId.Should().Be(representative.Id);
        result.Value.First().ClassRole.Should().Be("ClassRepresentative");
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task not_return_a_representative_of_another_class_as_a_member()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        User parent = UserMother.WithName(StringMother.Random("name"));
        User otherClassRepresentative = UserMother.Representative(GuidMother.Random());
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(parent.Id);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, parent.Id));
        this.ShouldSearchUsers(parent, otherClassRepresentative);

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Value.Select(member => member.UserId).Should().Equal(parent.Id);
    }

    [Test]
    public async Task return_class_members_forbidden_when_the_requester_is_not_a_member()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        User parent = UserMother.WithName(StringMother.Random("name"));
        User outsider = UserMother.WithName(StringMother.Random("name"));
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(outsider.Id);
        this.ShouldFindEnrollments(
            classId,
            EnrollmentResponseMother.OfStatus(classId, studentId, EnrollmentResponseMother.CONFIRMED));
        this.ShouldFindGuardians((studentId, parent.Id));
        this.ShouldSearchUsers(parent, outsider);

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.ClassMembersForbidden);
    }

    [Test]
    public async Task return_not_found_without_reading_enrollments_when_the_class_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Error.Should().Be(ClassErrors.NotFound);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<EnrollmentResponse>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_unauthenticated_without_any_read_when_there_is_no_current_user()
    {
        Guid classId = GuidMother.Random();
        this.ShouldHaveNoCurrentUser();

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        this.ShouldNotHaveSearched();
    }

    [Test]
    public void carry_only_the_user_id_the_name_and_the_role_in_the_response()
    {
        IEnumerable<string> properties = typeof(UserClassMemberResponse)
            .GetProperties()
            .Select(property => property.Name);

        properties.Should().BeEquivalentTo("UserId", "FullName", "ClassRole");
    }

    private Task<Result<IReadOnlyCollection<UserClassMemberResponse>>> Ask(Guid classId)
        => this._useCase.ExecuteAsync(
            UserClassMemberFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);
}
