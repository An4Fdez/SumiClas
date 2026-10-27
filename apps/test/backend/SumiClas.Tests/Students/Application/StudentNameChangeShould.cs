namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;

[TestFixture]
public class StudentNameChangeShould : StudentsUnitTestCase
{
    private readonly StudentNameChange.Change _useCase;

    public StudentNameChangeShould()
    {
        this._useCase = new StudentNameChange.Change(this.Repository.Object, this.UserProvider.Object);
    }

    [Test]
    public async Task rename_the_student_when_the_current_user_is_a_guardian_and_leave_the_guardians_alone()
    {
        Guid currentId = GuidMother.Random();
        Guid otherId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(otherId, currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        await this._useCase.ExecuteAsync(
            StudentNameChangeCommandMother.With(student.Id, " Lucia ", " Perez ", " Gil "));

        student.Name.Should().Be(StudentNameMother.With("Lucia", "Perez", "Gil"));
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(otherId), GuardianLinkMother.Father(currentId));
    }

    [Test]
    public async Task drop_the_second_surname_when_the_new_name_has_none()
    {
        Guid currentId = GuidMother.Random();
        Student student = new(
            GuidMother.Random(),
            StudentNameMother.With("Lucia", "Perez", "Gil"),
            [GuardianLinkMother.Mother(currentId)]);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        await this._useCase.ExecuteAsync(StudentNameChangeCommandMother.With(student.Id, "Lucia", "Perez"));

        student.Name.SecondSurname.Should().BeNull();
    }

    [Test]
    public async Task throw_guardian_access_forbidden_and_keep_the_name_when_the_current_user_is_not_a_guardian()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random(), GuidMother.Random());
        StudentName previousName = student.Name;
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(StudentNameChangeCommandMother.With(student.Id));

        await act.Should().ThrowAsync<StudentExceptions.GuardianAccessForbidden>();
        student.Name.Should().Be(previousName);
    }

    [Test]
    public async Task throw_not_found_when_the_student_does_not_exist()
    {
        this.ShouldFetchStudents(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(StudentNameChangeCommandMother.With(GuidMother.Random()));

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
    }

    [Test]
    public async Task throw_name_required_and_keep_the_name_when_the_first_name_is_blank()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        StudentName previousName = student.Name;
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentNameChangeCommandMother.With(student.Id, firstName: "  "));

        await act.Should().ThrowAsync<StudentExceptions.NameRequired>();
        student.Name.Should().Be(previousName);
    }

    [Test]
    public async Task throw_first_surname_required_and_keep_the_name_when_the_first_surname_is_blank()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        StudentName previousName = student.Name;
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentNameChangeCommandMother.With(student.Id, firstSurname: "  "));

        await act.Should().ThrowAsync<StudentExceptions.FirstSurnameRequired>();
        student.Name.Should().Be(previousName);
    }

    [Test]
    public async Task throw_unauthenticated_without_fetching_the_student_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(StudentNameChangeCommandMother.With(GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveFetched();
    }
}
