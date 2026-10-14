namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserClassMemberFinderByClassShould : UsersUnitTestCase
{
    private readonly List<User> _users = [];
    private readonly List<EnrollmentResponse> _enrollments = [];
    private readonly Dictionary<Guid, List<Guid>> _guardiansByStudent = [];
    private readonly UserClassMemberFinderByClass.Finder _useCase;

    public UserClassMemberFinderByClassShould()
    {
        this._useCase = new UserClassMemberFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [SetUp]
    public void SetUp()
    {
        this._users.Clear();
        this._enrollments.Clear();
        this._guardiansByStudent.Clear();
    }

    [Test]
    public async Task return_guardians_and_representatives_with_the_representatives_first_then_by_name()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        string anaName = StringMother.Random("Ana");
        string representativeName = StringMother.Random("Marta");
        string zoeName = StringMother.Random("zoe");
        User zoe = this.AddUser(classId, zoeName);
        User ana = this.AddUser(classId, anaName);
        User representative = this.AddUser(classId, representativeName, representative: true);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, zoe.Id);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, ana.Id, representative.Id);
        this.ShouldResolveCurrentUser(ana.Id);
        this.ShouldSearchUsers(this._users.ToArray());
        this.ShouldFindStoredEnrollments();
        this.ShouldFindStoredGuardians();

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
        this.ShouldFindClass(classId);
        User parent = this.AddUser(classId);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, parent.Id);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, parent.Id);
        this.ShouldResolveCurrentUser(parent.Id);
        this.ShouldSearchUsers(this._users.ToArray());
        this.ShouldFindStoredEnrollments();
        this.ShouldFindStoredGuardians();

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Value.Select(member => member.UserId).Should().Equal(parent.Id);
    }

    [Test]
    public async Task not_return_the_guardian_of_a_suggested_enrollment_as_a_member()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        User confirmedParent = this.AddUser(classId);
        User suggestedParent = this.AddUser(classId);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, confirmedParent.Id);
        this.AddEnrollment(classId, EnrollmentResponseMother.SUGGESTED, suggestedParent.Id);
        this.ShouldResolveCurrentUser(confirmedParent.Id);
        this.ShouldSearchUsers(this._users.ToArray());
        this.ShouldFindStoredEnrollments();
        this.ShouldFindStoredGuardians();

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.Value.Select(member => member.UserId).Should().Equal(confirmedParent.Id);
    }

    [Test]
    public async Task include_a_representative_without_children_and_list_them_first()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        User parent = this.AddUser(classId);
        User representative = this.AddUser(classId, representative: true);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, parent.Id);
        this.ShouldResolveCurrentUser(representative.Id);
        this.ShouldSearchUsers(this._users.ToArray());
        this.ShouldFindStoredEnrollments();
        this.ShouldFindStoredGuardians();

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.IsSuccess.Should().BeTrue();
        result.Value.First().UserId.Should().Be(representative.Id);
        result.Value.First().ClassRole.Should().Be("ClassRepresentative");
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task return_class_members_forbidden_when_the_requester_is_not_a_member()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        User parent = this.AddUser(classId);
        User outsider = this.AddUser(classId);
        this.AddEnrollment(classId, EnrollmentResponseMother.CONFIRMED, parent.Id);
        this.ShouldResolveCurrentUser(outsider.Id);
        this.ShouldSearchUsers(this._users.ToArray());
        this.ShouldFindStoredEnrollments();
        this.ShouldFindStoredGuardians();

        Result<IReadOnlyCollection<UserClassMemberResponse>> result = await this.Ask(classId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.ClassMembersForbidden);
    }

    [Test]
    public async Task return_not_found_without_reading_enrollments_when_the_class_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        User parent = this.AddUser(classId);
        this.ShouldResolveCurrentUser(parent.Id);
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

    private User AddUser(Guid classId, string? fullName = null, bool representative = false)
    {
        string name = fullName ?? StringMother.Random("name");
        User user = representative
            ? UserMother.Representative(classId, name)
            : UserMother.WithName(name);
        this._users.Add(user);
        return user;
    }

    private Guid AddEnrollment(Guid classId, string status, params Guid[] guardianIds)
    {
        Guid studentId = GuidMother.Random();
        this._enrollments.Add(EnrollmentResponseMother.OfStatus(classId, studentId, status));
        this._guardiansByStudent[studentId] = guardianIds.ToList();
        return studentId;
    }

    private Task<Result<IReadOnlyCollection<UserClassMemberResponse>>> Ask(Guid classId)
        => this._useCase.ExecuteAsync(
            UserClassMemberFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

    private void ShouldFindStoredEnrollments()
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<EnrollmentResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => this._enrollments);

    private void ShouldFindStoredGuardians()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<Guid>>>(), It.IsAny<CancellationToken>()))
            .Returns<IQuery<IReadOnlyCollection<Guid>>, CancellationToken>((query, _) =>
            {
                IReadOnlyCollection<Guid> studentIds = ((StudentGuardianFinderByStudents.Query)query).StudentIds;
                IReadOnlyCollection<Guid> guardianIds = studentIds
                    .Where(this._guardiansByStudent.ContainsKey)
                    .SelectMany(studentId => this._guardiansByStudent[studentId])
                    .Distinct()
                    .ToList();
                return Task.FromResult(guardianIds);
            });
}
