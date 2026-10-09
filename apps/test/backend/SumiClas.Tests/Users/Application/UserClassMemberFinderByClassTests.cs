using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserClassMemberFinderByClassTests
{
    private static readonly Guid ClassId = Guid.Parse("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1");

    private Mock<IUserRepository> _users = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IUserProvider> _userProvider = null!;
    private List<User> _store = null!;
    private List<EnrollmentResponse> _enrollments = null!;
    private Dictionary<Guid, List<Guid>> _guardiansByStudent = null!;
    private Result<ClassResponse> _classResult = null!;
    private Guid _currentUserId;
    private UserClassMemberFinderByClass.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        _enrollments = [];
        _guardiansByStudent = [];
        _classResult = Result<ClassResponse>.Success(new ClassResponse(ClassId, Guid.NewGuid(), "3A"));
        _currentUserId = Guid.Empty;

        _userProvider = new Mock<IUserProvider>();
        _userProvider.Setup(provider => provider.GetCurrentUserId()).Returns(() => Result<Guid>.Success(_currentUserId));

        _users = new Mock<IUserRepository>();
        _users
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<User>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<User>>(_store.Where(specification.Criteria.Compile()).ToList()));

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _classResult);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<EnrollmentResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _enrollments);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<Guid>>>(), It.IsAny<CancellationToken>()))
            .Returns<IQuery<IReadOnlyCollection<Guid>>, CancellationToken>((query, _) =>
            {
                var studentIds = ((StudentGuardianFinderByStudents.Query)query).StudentIds;
                IReadOnlyCollection<Guid> guardianIds = studentIds
                    .Where(_guardiansByStudent.ContainsKey)
                    .SelectMany(studentId => _guardiansByStudent[studentId])
                    .Distinct()
                    .ToList();
                return Task.FromResult(guardianIds);
            });

        _handler = new UserClassMemberFinderByClass.Handler(_users.Object, _queryBus.Object, _userProvider.Object);
    }

    private User AddUser(string fullName, bool representative = false)
    {
        var id = Guid.NewGuid();
        var user = User.Register(id, fullName, Email.Create($"{id:N}@example.com").Value).Value;
        if (representative)
        {
            user.AssignAsClassRepresentative(ClassId, 0);
        }

        _store.Add(user);
        return user;
    }

    private Guid AddEnrollment(string status, params Guid[] guardianIds)
    {
        var studentId = Guid.NewGuid();
        _enrollments.Add(new EnrollmentResponse(Guid.NewGuid(), studentId, ClassId, status, DateTimeOffset.UtcNow, null));
        _guardiansByStudent[studentId] = guardianIds.ToList();
        return studentId;
    }

    private Task<Result<IReadOnlyCollection<UserClassMemberResponse>>> Ask()
        => _handler.Handle(new UserClassMemberFinderByClass.Query(ClassId), CancellationToken.None);

    [Test]
    public async Task Handle_ReturnsGuardiansAndDelegates_DelegatesFirstThenByName()
    {
        var zoe = AddUser("zoe Pérez");
        var ana = AddUser("Ana Gil");
        var delegateUser = AddUser("Marta Ruiz", representative: true);
        AddEnrollment("Confirmed", zoe.Id);
        AddEnrollment("Confirmed", ana.Id, delegateUser.Id);
        _currentUserId = ana.Id;

        var result = await Ask();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Select(member => member.FullName), Is.EqualTo(new[] { "Marta Ruiz", "Ana Gil", "zoe Pérez" }));
        Assert.That(
            result.Value.Select(member => member.ClassRole),
            Is.EqualTo(new[] { "ClassRepresentative", "Participant", "Participant" }));
    }

    [Test]
    public async Task Handle_GuardianOfTwoChildren_AppearsOnce()
    {
        var parent = AddUser("Lucía Soto");
        AddEnrollment("Confirmed", parent.Id);
        AddEnrollment("Confirmed", parent.Id);
        _currentUserId = parent.Id;

        var result = await Ask();

        Assert.That(result.Value.Count(member => member.UserId == parent.Id), Is.EqualTo(1));
        Assert.That(result.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Handle_GuardianOfASuggestedEnrollment_IsNotAMember()
    {
        var confirmedParent = AddUser("Ana Gil");
        var suggestedParent = AddUser("Pedro Mora");
        AddEnrollment("Confirmed", confirmedParent.Id);
        AddEnrollment("Suggested", suggestedParent.Id);
        _currentUserId = confirmedParent.Id;

        var result = await Ask();

        Assert.That(result.Value.Select(member => member.UserId), Is.EqualTo(new[] { confirmedParent.Id }));
    }

    [Test]
    public async Task Handle_DelegateWithoutChildren_IsIncludedAndListedFirst()
    {
        var parent = AddUser("Ana Gil");
        var delegateUser = AddUser("Zacarías Vega", representative: true);
        AddEnrollment("Confirmed", parent.Id);
        _currentUserId = delegateUser.Id;

        var result = await Ask();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.First().UserId, Is.EqualTo(delegateUser.Id));
        Assert.That(result.Value.First().ClassRole, Is.EqualTo("ClassRepresentative"));
        Assert.That(result.Value, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Handle_WhenRequesterIsNotAMember_ReturnsClassMembersForbidden()
    {
        var parent = AddUser("Ana Gil");
        var outsider = AddUser("Carlos Ibáñez");
        AddEnrollment("Confirmed", parent.Id);
        _currentUserId = outsider.Id;

        var result = await Ask();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.ClassMembersForbidden));
    }

    [Test]
    public async Task Handle_WhenRequesterOnlyHasASuggestedEnrollment_ReturnsClassMembersForbidden()
    {
        var parent = AddUser("Ana Gil");
        var suggestedParent = AddUser("Pedro Mora");
        AddEnrollment("Confirmed", parent.Id);
        AddEnrollment("Suggested", suggestedParent.Id);
        _currentUserId = suggestedParent.Id;

        var result = await Ask();

        Assert.That(result.Error, Is.EqualTo(UserErrors.ClassMembersForbidden));
    }

    [Test]
    public async Task Handle_WhenClassDoesNotExist_ReturnsNotFoundWithoutReadingEnrollments()
    {
        var parent = AddUser("Ana Gil");
        _currentUserId = parent.Id;
        _classResult = Result<ClassResponse>.Failure(ClassErrors.NotFound);

        var result = await Ask();

        Assert.That(result.Error, Is.EqualTo(ClassErrors.NotFound));
        _queryBus.Verify(
            bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<EnrollmentResponse>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_WhenThereIsNoCurrentUser_ReturnsUnauthenticatedWithoutAnyRead()
    {
        _userProvider.Setup(provider => provider.GetCurrentUserId())
            .Returns(Result<Guid>.Failure(AuthenticationErrors.Unauthenticated));

        var result = await Ask();

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
        _queryBus.Verify(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()), Times.Never);
        _users.Verify(
            repository => repository.SearchAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_SkipsGuardianIdsThatNoLongerExist()
    {
        var parent = AddUser("Ana Gil");
        var deletedGuardianId = Guid.NewGuid();
        AddEnrollment("Confirmed", parent.Id, deletedGuardianId);
        _currentUserId = parent.Id;

        var result = await Ask();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Select(member => member.UserId), Is.EqualTo(new[] { parent.Id }));
    }

    [Test]
    public async Task Handle_ReadsOnlyGuardianIdsOfConfirmedStudents()
    {
        var parent = AddUser("Ana Gil");
        var confirmedStudent = AddEnrollment("Confirmed", parent.Id);
        AddEnrollment("Suggested", AddUser("Pedro Mora").Id);
        _currentUserId = parent.Id;

        await Ask();

        _queryBus.Verify(
            bus => bus.Send(
                It.Is<IQuery<IReadOnlyCollection<Guid>>>(query =>
                    ((StudentGuardianFinderByStudents.Query)query).StudentIds.SequenceEqual(new[] { confirmedStudent })),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Response_CarriesOnlyUserIdNameAndRole()
    {
        var properties = typeof(UserClassMemberResponse).GetProperties().Select(property => property.Name);

        Assert.That(properties, Is.EquivalentTo(new[] { "UserId", "FullName", "ClassRole" }));
    }
}
