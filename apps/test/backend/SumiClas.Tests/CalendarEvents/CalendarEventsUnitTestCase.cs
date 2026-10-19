namespace SumiClas.Tests.CalendarEvents;

using Moq;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Application;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class CalendarEventsUnitTestCase : UnitTestCase
{
    private readonly List<CalendarEvent> _added = [];

    protected Mock<ICalendarEventRepository> Repository { get; } = new();

    protected IReadOnlyList<CalendarEvent> Added => this._added;

    [SetUp]
    public void CalendarEventsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldFetchCalendarEvents(params CalendarEvent[] calendarEvents)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => calendarEvents.FirstOrDefault(calendarEvent => calendarEvent.Id == id));

    protected void ShouldSearchCalendarEvents(params CalendarEvent[] calendarEvents)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<CalendarEvent>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<CalendarEvent> specification, CancellationToken _)
                => InMemorySearch.Run(calendarEvents, specification));

    protected void ShouldSearchNoCalendarEvents()
        => this.ShouldSearchCalendarEvents();

    protected void ShouldCaptureAddedCalendarEvents()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<CalendarEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CalendarEvent, CancellationToken>((calendarEvent, _) => this._added.Add(calendarEvent))
            .Returns(Task.CompletedTask);

    protected void ShouldHaveSearched(int times)
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<CalendarEvent>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(times));

    protected void ShouldFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(UserResponseMother.With(userId, StringMother.Random("name"))));

    protected void ShouldNotFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.With(classId, schoolYearId)));

    protected void ShouldNotFindClass()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Failure(ClassErrors.NotFound));

    protected void ShouldEnsureSchoolYearIsActive(Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldFailEnsuringSchoolYearIsActive(Guid schoolYearId, Error error)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

    protected void ShouldFindClass(Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.WithId(classId)));

    protected void ShouldEnsureStudentGuardian(Guid studentId, Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<StudentGuardianEnsure.Query>(query => query.StudentId == studentId && query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldFailEnsuringStudentGuardian(Guid studentId, Guid userId, Error error)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<StudentGuardianEnsure.Query>(query => query.StudentId == studentId && query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

    protected void ShouldAllowClassAccess(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassAccessEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldDenyClassAccess(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassAccessEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(UserErrors.ClassAccessForbidden));

    protected void ShouldAllowClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassRepresentativeEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldDenyClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassRepresentativeEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(UserErrors.ClassRepresentativeForbidden));
}
