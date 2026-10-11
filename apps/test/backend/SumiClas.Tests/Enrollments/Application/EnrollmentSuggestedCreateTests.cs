using Moq;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Tests.Enrollments.Application;

[TestFixture]
public class EnrollmentSuggestedCreateTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SchoolId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid OldYearId = Guid.Parse("c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1");
    private static readonly Guid NewYearId = Guid.Parse("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2");
    private static readonly Guid OldClassId = Guid.Parse("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1");
    private static readonly Guid NewClassId = Guid.Parse("d2d2d2d2-d2d2-d2d2-d2d2-d2d2d2d2d2d2");
    private static readonly Guid OtherOldClassId = Guid.Parse("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3");

    private Mock<IEnrollmentRepository> _enrollments = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IUserProvider> _userProvider = null!;
    private Mock<IClock> _clock = null!;
    private List<Enrollment> _store = null!;
    private List<Enrollment> _added = null!;
    private EnrollmentSuggestedCreate.Create _useCase = null!;
    private EnrollmentSuggestedCreate.Handler _handler = null!;
    private bool _isRepresentative;
    private Guid _previousSchoolId;
    private Result _newYearActive = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        _added = [];
        _isRepresentative = true;
        _previousSchoolId = SchoolId;
        _newYearActive = Result.Success();

        _clock = new Mock<IClock>();
        _clock.Setup(clock => clock.UtcNow).Returns(new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero));

        _userProvider = new Mock<IUserProvider>();
        _userProvider.Setup(provider => provider.GetCurrentUserId()).Returns(Result<Guid>.Success(CurrentUserId));

        _enrollments = new Mock<IEnrollmentRepository>();
        _enrollments
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<Enrollment>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<Enrollment>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<Enrollment>>(_store.Where(specification.Criteria.Compile()).ToList()));
        _enrollments
            .Setup(repository => repository.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((enrollment, _) =>
            {
                _added.Add(enrollment);
                _store.Add(enrollment);
            })
            .Returns(Task.CompletedTask);

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .Returns<IQuery<Result<ClassResponse>>, CancellationToken>((query, _) =>
            {
                var classId = ((ClassFinderById.Query)query).ClassId;
                var response = classId == NewClassId
                    ? new ClassResponse(NewClassId, NewYearId, "3A")
                    : classId == OtherOldClassId
                        ? new ClassResponse(OtherOldClassId, NewYearId, "3B")
                        : new ClassResponse(OldClassId, OldYearId, "2A");
                return Task.FromResult(Result<ClassResponse>.Success(response));
            });
        _queryBus
            .Setup(bus => bus.Send(It.Is<IQuery<Result>>(query => query is SchoolYearEnsureIsActive.Query), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _newYearActive);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _isRepresentative);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<SchoolYearResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<SchoolYearResponse>
            {
                new(OldYearId, _previousSchoolId, "2025-26", new DateOnly(2025, 9, 1), new DateOnly(2026, 6, 30), "Archived"),
                new(NewYearId, SchoolId, "2026-27", new DateOnly(2026, 9, 1), new DateOnly(2027, 6, 30), "Active"),
            });
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassResponse> { new(NewClassId, NewYearId, "3A") });

        _useCase = new EnrollmentSuggestedCreate.Create(_enrollments.Object, _queryBus.Object, _userProvider.Object, _clock.Object);
        _handler = new EnrollmentSuggestedCreate.Handler(_useCase);
    }

    private Guid AddConfirmed(Guid classId)
    {
        var studentId = Guid.NewGuid();
        _store.Add(Enrollment.Create(Guid.NewGuid(), studentId, classId, _clock.Object).Value);
        return studentId;
    }

    private void AddSuggested(Guid studentId, Guid classId)
        => _store.Add(Enrollment.CreateSuggested(Guid.NewGuid(), studentId, classId, _clock.Object).Value);

    [Test]
    public async Task UseCase_WithConfirmedStudents_CreatesOneSuggestedEnrollmentEach()
    {
        AddConfirmed(OldClassId);
        AddConfirmed(OldClassId);
        AddConfirmed(OldClassId);

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(3, 0)));
        Assert.That(_added, Has.Count.EqualTo(3));
        Assert.That(_added.All(enrollment => enrollment.Status == EnrollmentStatus.Suggested && enrollment.ClassId == NewClassId), Is.True);
        Assert.That(_added.Select(enrollment => enrollment.Id).Distinct().Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task UseCase_OnlyCopiesConfirmedEnrollmentsOfThePreviousClass()
    {
        AddConfirmed(OldClassId);
        AddSuggested(Guid.NewGuid(), OldClassId);

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(1, 0)));
    }

    [Test]
    public async Task UseCase_WhenCurrentUserIsNotARepresentative_ReturnsSuggestForbidden()
    {
        AddConfirmed(OldClassId);
        _isRepresentative = false;

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EnrollmentErrors.SuggestForbidden));
        Assert.That(_added, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithoutCurrentUser_ReturnsUnauthenticated()
    {
        _userProvider.Setup(provider => provider.GetCurrentUserId()).Returns(Result<Guid>.Failure(AuthenticationErrors.Unauthenticated));

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
    }

    [Test]
    public async Task UseCase_WithTheSameClass_ReturnsSameClassWithoutAnyLookup()
    {
        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, NewClassId));

        Assert.That(result.Error, Is.EqualTo(EnrollmentErrors.SameClass));
        _queryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task UseCase_WithEmptyClassId_ReturnsClassRequired()
    {
        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(Guid.Empty, OldClassId));

        Assert.That(result.Error, Is.EqualTo(EnrollmentErrors.ClassRequired));
    }

    [Test]
    public async Task UseCase_WhenNewSchoolYearIsArchived_ReturnsThatFailure()
    {
        var archived = new Error("SchoolYear.Archived", "El curso escolar está archivado.");
        _newYearActive = Result.Failure(archived);

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Error, Is.EqualTo(archived));
        Assert.That(_added, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithClassesOfTheSameSchoolYear_ReturnsSameSchoolYear()
    {
        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OtherOldClassId));

        Assert.That(result.Error, Is.EqualTo(EnrollmentErrors.SameSchoolYear));
    }

    [Test]
    public async Task UseCase_WithClassesOfDifferentSchools_ReturnsDifferentSchool()
    {
        AddConfirmed(OldClassId);
        _previousSchoolId = Guid.NewGuid();

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Error, Is.EqualTo(EnrollmentErrors.DifferentSchool));
        Assert.That(_added, Is.Empty);
    }

    [Test]
    public async Task UseCase_SkipsStudentsAlreadyInTheNewClass()
    {
        var alreadySuggested = AddConfirmed(OldClassId);
        AddConfirmed(OldClassId);
        AddSuggested(alreadySuggested, NewClassId);

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(1, 1)));
    }

    [Test]
    public async Task UseCase_SkipsStudentsWithAConfirmedEnrollmentInTheNewSchoolYear()
    {
        var movedByTheGuardian = AddConfirmed(OldClassId);
        AddConfirmed(OldClassId);
        _store.Add(Enrollment.Create(Guid.NewGuid(), movedByTheGuardian, NewClassId, _clock.Object).Value);

        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(1, 1)));
    }

    [Test]
    public async Task UseCase_WhenRunTwice_CreatesNothingTheSecondTime()
    {
        AddConfirmed(OldClassId);
        AddConfirmed(OldClassId);

        var first = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));
        var second = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(first.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(2, 0)));
        Assert.That(second.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(0, 2)));
        Assert.That(_added, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UseCase_WithNoConfirmedStudents_ReturnsZeroes()
    {
        var result = await _useCase.ExecuteAsync(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId));

        Assert.That(result.Value, Is.EqualTo(new EnrollmentSuggestedCreateResponse(0, 0)));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        AddConfirmed(OldClassId);

        var result = await _handler.Handle(new EnrollmentSuggestedCreate.Command(NewClassId, OldClassId), CancellationToken.None);

        Assert.That(result.Value.Created, Is.EqualTo(1));
    }
}
