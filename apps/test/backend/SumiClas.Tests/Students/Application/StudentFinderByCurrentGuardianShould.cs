namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Enrollments.Application;
using SumiClas.Tests.Schools.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;

[TestFixture]
public class StudentFinderByCurrentGuardianShould : StudentsUnitTestCase
{
    private readonly StudentFinderByCurrentGuardian.Finder _useCase;
    private readonly StudentFinderByCurrentGuardian.Handler _handler;

    public StudentFinderByCurrentGuardianShould()
    {
        this._useCase = new StudentFinderByCurrentGuardian.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
        this._handler = new StudentFinderByCurrentGuardian.Handler(this._useCase);
    }

    [Test]
    public async Task return_the_child_with_the_class_and_the_school_of_its_confirmed_enrollment()
    {
        Guid guardianId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        ClassResponse schoolClass = ClassResponseMother.With(GuidMother.Random(), schoolYearId);
        SchoolResponse school = SchoolResponseMother.WithId(GuidMother.Random());
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, schoolClass.Id));
        this.ShouldFindClasses(schoolClass);
        this.ShouldFindSchoolYears(SchoolYearResponseMother.Active(schoolYearId, school.Id));
        this.ShouldFindSchools(school);

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(ChildResponseMother.InSchool(student.Id, "Ana", "L.", schoolClass, school));
    }

    [Test]
    public async Task return_the_children_of_which_the_current_user_is_a_secondary_guardian()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(GuidMother.Random(), guardianId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindNothingElsewhere();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Select(child => child.StudentId).Should().Equal(student.Id);
    }

    [Test]
    public async Task return_only_the_children_of_the_current_user()
    {
        Guid guardianId = GuidMother.Random();
        Student own = StudentMother.GuardedBy(guardianId);
        Student other = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldReturn(other, own);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindNothingElsewhere();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Select(child => child.StudentId).Should().Equal(own.Id);
    }

    [Test]
    public async Task order_the_children_by_first_name()
    {
        Guid guardianId = GuidMother.Random();
        this.ShouldReturn(
            StudentMother.Named(guardianId, "Zoe", "Perez"),
            StudentMother.Named(guardianId, "Ana", "Perez"),
            StudentMother.Named(guardianId, "Marta", "Perez"));
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindNothingElsewhere();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Select(child => child.FirstName).Should().Equal("Ana", "Marta", "Zoe");
    }

    [Test]
    public async Task return_the_child_without_class_when_it_has_no_enrollments()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindNothingElsewhere();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L."));
    }

    [Test]
    public async Task ignore_the_suggested_enrollments_and_not_ask_for_their_classes()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.SuggestedFor(student.Id, GuidMother.Random()));
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L."));
        this.QueryBus.Verify(
            bus => bus.Send(It.Is<ClassFinderByIds.Query>(query => query.ClassIds.Count == 0), CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task return_the_child_without_class_when_its_class_belongs_to_an_archived_school_year()
    {
        Guid guardianId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        ClassResponse schoolClass = ClassResponseMother.With(GuidMother.Random(), schoolYearId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, schoolClass.Id));
        this.ShouldFindClasses(schoolClass);
        this.ShouldFindSchoolYears(SchoolYearResponseMother.Archived(schoolYearId, GuidMother.Random()));
        this.ShouldFindSchools();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L."));
    }

    [Test]
    public async Task return_the_child_without_class_when_the_class_of_its_enrollment_no_longer_exists()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, GuidMother.Random()));
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L."));
    }

    [Test]
    public async Task return_the_class_without_school_when_the_school_is_not_found()
    {
        Guid guardianId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        ClassResponse schoolClass = ClassResponseMother.With(GuidMother.Random(), schoolYearId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, schoolClass.Id));
        this.ShouldFindClasses(schoolClass);
        this.ShouldFindSchoolYears(SchoolYearResponseMother.Active(schoolYearId, GuidMother.Random()));
        this.ShouldFindSchools();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Should().Equal(ChildResponseMother.InClass(student.Id, "Ana", "L.", schoolClass));
    }

    [Test]
    public async Task prefer_the_class_of_the_active_school_year_over_a_confirmed_enrollment_of_an_archived_one()
    {
        Guid guardianId = GuidMother.Random();
        Guid archivedYearId = GuidMother.Random();
        Guid activeYearId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        ClassResponse archivedClass = ClassResponseMother.With(GuidMother.Random(), archivedYearId);
        ClassResponse activeClass = ClassResponseMother.With(GuidMother.Random(), activeYearId);
        SchoolResponse school = SchoolResponseMother.WithId(GuidMother.Random());
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(
            EnrollmentResponseMother.ConfirmedFor(student.Id, archivedClass.Id),
            EnrollmentResponseMother.ConfirmedFor(student.Id, activeClass.Id));
        this.ShouldFindClasses(archivedClass, activeClass);
        this.ShouldFindSchoolYears(
            SchoolYearResponseMother.Archived(archivedYearId, school.Id),
            SchoolYearResponseMother.Active(activeYearId, school.Id));
        this.ShouldFindSchools(school);

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Value.Should().Equal(ChildResponseMother.InSchool(student.Id, "Ana", "L.", activeClass, school));
    }

    [Test]
    public async Task ask_only_once_for_each_class_school_year_and_active_school_of_the_confirmed_enrollments()
    {
        Guid guardianId = GuidMother.Random();
        Guid activeYearId = GuidMother.Random();
        Guid archivedYearId = GuidMother.Random();
        Guid activeSchoolId = GuidMother.Random();
        Student first = StudentMother.GuardedBy(guardianId);
        Student sibling = StudentMother.GuardedBy(guardianId);
        Student archived = StudentMother.GuardedBy(guardianId);
        Student suggested = StudentMother.GuardedBy(guardianId);
        ClassResponse activeClass = ClassResponseMother.With(GuidMother.Random(), activeYearId);
        ClassResponse archivedClass = ClassResponseMother.With(GuidMother.Random(), archivedYearId);
        this.ShouldReturn(first, sibling, archived, suggested);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(
            EnrollmentResponseMother.ConfirmedFor(first.Id, activeClass.Id),
            EnrollmentResponseMother.ConfirmedFor(sibling.Id, activeClass.Id),
            EnrollmentResponseMother.ConfirmedFor(archived.Id, archivedClass.Id),
            EnrollmentResponseMother.SuggestedFor(suggested.Id, GuidMother.Random()));
        this.ShouldFindClasses(activeClass, archivedClass);
        this.ShouldFindSchoolYears(
            SchoolYearResponseMother.Active(activeYearId, activeSchoolId),
            SchoolYearResponseMother.Archived(archivedYearId, GuidMother.Random()));
        this.ShouldFindSchools(SchoolResponseMother.WithId(activeSchoolId));

        await this.FindChildren();

        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<EnrollmentFinderByStudents.Query>(query => HasIds(
                    query.StudentIds, first.Id, sibling.Id, archived.Id, suggested.Id)),
                CancellationToken.None),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<ClassFinderByIds.Query>(query => HasIds(query.ClassIds, activeClass.Id, archivedClass.Id)),
                CancellationToken.None),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<SchoolYearFinderByIds.Query>(query => HasIds(query.SchoolYearIds, activeYearId, archivedYearId)),
                CancellationToken.None),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<SchoolFinderByIds.Query>(query => HasIds(query.SchoolIds, activeSchoolId)),
                CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task return_empty_without_asking_other_modules_when_the_current_user_has_no_students()
    {
        Guid guardianId = GuidMother.Random();
        this.ShouldReturn(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldResolveCurrentUser(guardianId);

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_unauthenticated_without_searching_or_asking_when_there_is_no_current_user()
    {
        this.ShouldReturn(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldHaveNoCurrentUser();

        Result<IReadOnlyCollection<ChildResponse>> result = await this.FindChildren();

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        this.ShouldHaveNoCurrentUser();

        Result<IReadOnlyCollection<ChildResponse>> result = await this._handler.Handle(
            new StudentFinderByCurrentGuardian.Query(),
            CancellationToken.None);

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }

    private static bool HasIds(IReadOnlyCollection<Guid> actual, params Guid[] expected)
        => actual.Count == expected.Length && expected.All(actual.Contains);

    private Task<Result<IReadOnlyCollection<ChildResponse>>> FindChildren()
        => this._useCase.ExecuteAsync(new StudentFinderByCurrentGuardian.Query());

    private void ShouldFindNothingElsewhere()
    {
        this.ShouldFindEnrollments();
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();
    }

    private void ShouldFindEnrollments(params EnrollmentResponse[] enrollments)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<EnrollmentFinderByStudents.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

    private void ShouldFindClasses(params ClassResponse[] classes)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<ClassFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classes);

    private void ShouldFindSchoolYears(params SchoolYearResponse[] schoolYears)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<SchoolYearFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(schoolYears);

    private void ShouldFindSchools(params SchoolResponse[] schools)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<SchoolFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(schools);
}
