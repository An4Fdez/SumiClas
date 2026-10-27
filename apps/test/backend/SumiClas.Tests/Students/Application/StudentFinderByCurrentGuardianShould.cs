namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Application;
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

    public StudentFinderByCurrentGuardianShould()
    {
        this._useCase = new StudentFinderByCurrentGuardian.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_child_with_the_class_and_the_school_of_its_confirmed_enrollment()
    {
        Guid guardianId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        ClassResponse schoolClass = ClassResponseMother.With(GuidMother.Random(), schoolYearId);
        SchoolResponse school = SchoolResponseMother.WithId(GuidMother.Random());
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, schoolClass.Id));
        this.ShouldFindClasses(schoolClass);
        this.ShouldFindSchoolYears(SchoolYearResponseMother.Active(schoolYearId, school.Id));
        this.ShouldFindSchools(school);

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();
        result.Should().Equal(ChildResponseMother.InSchool(student.Id, "Ana", "L.", "Lopez", null, schoolClass, school));
    }

    [Test]
    public async Task return_the_full_surnames_of_the_child_for_its_guardian()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez", "Garcia");
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments();
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L.", "Lopez", "Garcia"));
    }

    [Test]
    public async Task return_only_the_children_of_which_the_current_user_is_a_main_or_secondary_guardian()
    {
        Guid guardianId = GuidMother.Random();
        Student own = StudentMother.GuardedBy(guardianId);
        Student secondary = StudentMother.GuardedBy(GuidMother.Random(), guardianId);
        Student other = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldSearchStudents(other, own, secondary);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments();
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Select(child => child.StudentId).Should().BeEquivalentTo([own.Id, secondary.Id]);
    }

    [Test]
    public async Task order_the_children_by_first_name()
    {
        Guid guardianId = GuidMother.Random();
        this.ShouldSearchStudents(
            StudentMother.Named(guardianId, "Zoe", "Perez"),
            StudentMother.Named(guardianId, "Ana", "Perez"),
            StudentMother.Named(guardianId, "Marta", "Perez"));
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments();
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Select(child => child.FirstName).Should().Equal("Ana", "Marta", "Zoe");
    }

    [Test]
    public async Task ignore_the_suggested_enrollments_and_not_ask_for_their_classes()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.SuggestedFor(student.Id, GuidMother.Random()));
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L.", "Lopez", null));
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
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, schoolClass.Id));
        this.ShouldFindClasses(schoolClass);
        this.ShouldFindSchoolYears(SchoolYearResponseMother.Archived(schoolYearId, GuidMother.Random()));
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L.", "Lopez", null));
    }

    [Test]
    public async Task return_the_child_without_class_when_the_class_of_its_enrollment_no_longer_exists()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, GuidMother.Random()));
        this.ShouldFindClasses();
        this.ShouldFindSchoolYears();
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Should().Equal(ChildResponseMother.WithoutClass(student.Id, "Ana", "L.", "Lopez", null));
    }

    [Test]
    public async Task return_the_class_without_school_when_the_school_is_not_found()
    {
        Guid guardianId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Student student = StudentMother.Named(guardianId, "Ana", "Lopez");
        ClassResponse schoolClass = ClassResponseMother.With(GuidMother.Random(), schoolYearId);
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(EnrollmentResponseMother.ConfirmedFor(student.Id, schoolClass.Id));
        this.ShouldFindClasses(schoolClass);
        this.ShouldFindSchoolYears(SchoolYearResponseMother.Active(schoolYearId, GuidMother.Random()));
        this.ShouldFindSchools();

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Should().Equal(ChildResponseMother.InClass(student.Id, "Ana", "L.", "Lopez", null, schoolClass));
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
        this.ShouldSearchStudents(student);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindEnrollments(
            EnrollmentResponseMother.ConfirmedFor(student.Id, archivedClass.Id),
            EnrollmentResponseMother.ConfirmedFor(student.Id, activeClass.Id));
        this.ShouldFindClasses(archivedClass, activeClass);
        this.ShouldFindSchoolYears(
            SchoolYearResponseMother.Archived(archivedYearId, school.Id),
            SchoolYearResponseMother.Active(activeYearId, school.Id));
        this.ShouldFindSchools(school);

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();

        result.Should().Equal(ChildResponseMother.InSchool(student.Id, "Ana", "L.", "Lopez", null, activeClass, school));
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
        this.ShouldSearchStudents(first, sibling, archived, suggested);
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
        this.ShouldSearchStudents(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldResolveCurrentUser(guardianId);

        IReadOnlyCollection<ChildResponse> result = await this.FindChildren();
        result.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_unauthenticated_without_searching_or_asking_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this.FindChildren();

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    private static bool HasIds(IReadOnlyCollection<Guid> actual, params Guid[] expected)
        => actual.Count == expected.Length && expected.All(actual.Contains);

    private Task<IReadOnlyCollection<ChildResponse>> FindChildren()
        => this._useCase.ExecuteAsync(new StudentFinderByCurrentGuardian.Query());

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
