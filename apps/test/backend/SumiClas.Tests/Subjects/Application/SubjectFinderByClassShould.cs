namespace SumiClas.Tests.Subjects.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Subjects.Domain;
using SumiClas.Users.Domain;

[TestFixture]
public class SubjectFinderByClassShould : SubjectsUnitTestCase
{
    private readonly SubjectFinderByClass.Finder _useCase;

    public SubjectFinderByClassShould()
    {
        this._useCase = new SubjectFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_subjects_of_the_class_only_ordered_by_label_with_their_id_class_and_label()
    {
        Guid classId = GuidMother.Random();
        Subject first = SubjectMother.With(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            classId,
            StringMother.Random("a"));
        Subject second = SubjectMother.With(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            classId,
            StringMother.Random("b"));
        Subject third = SubjectMother.With(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            classId,
            StringMother.Random("c"));
        this.ShouldSearchSubjects(third, SubjectMother.InClass(GuidMother.Random()), first, second);
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);

        IReadOnlyCollection<SubjectResponse> result = await this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));
        result.Should().Equal(
            SubjectResponseMother.With(first.Id, classId, first.Label),
            SubjectResponseMother.With(second.Id, classId, second.Label),
            SubjectResponseMother.With(third.Id, classId, third.Label));
        this.ShouldHaveAskedForClass(classId);
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_unauthenticated_before_looking_for_the_class_when_there_is_no_current_user()
    {
        this.ShouldNotFindClass();
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_unauthenticated_without_reading_when_there_is_no_current_user()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_class_not_found_without_searching_when_the_class_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }
}
