namespace SumiClas.Tests.Subjects.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Subjects.Domain;

[TestFixture]
public class SubjectFinderByClassShould : SubjectsUnitTestCase
{
    private readonly SubjectFinderByClass.Finder _useCase;

    public SubjectFinderByClassShould()
    {
        this._useCase = new SubjectFinderByClass.Finder(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task return_the_subjects_of_the_class_with_their_id_class_and_label()
    {
        Guid classId = GuidMother.Random();
        Subject subject = SubjectMother.InClass(classId);
        this.ShouldSearchSubjects(subject);
        this.ShouldFindClass(classId);

        Result<IReadOnlyCollection<SubjectResponse>> result = await this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(SubjectResponseMother.With(subject.Id, classId, subject.Label));
    }

    [Test]
    public async Task return_the_subjects_ordered_by_label_whatever_the_stored_order()
    {
        Guid classId = GuidMother.Random();
        Subject first = SubjectMother.WithLabel(classId, StringMother.Random("a"));
        Subject second = SubjectMother.WithLabel(classId, StringMother.Random("b"));
        Subject third = SubjectMother.WithLabel(classId, StringMother.Random("c"));
        this.ShouldSearchSubjects(third, first, second);
        this.ShouldFindClass(classId);

        Result<IReadOnlyCollection<SubjectResponse>> result = await this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        result.Value.Select(response => response.Id).Should().Equal(first.Id, second.Id, third.Id);
    }

    [Test]
    public async Task leave_out_the_subjects_of_other_classes()
    {
        Guid classId = GuidMother.Random();
        Subject own = SubjectMother.InClass(classId);
        this.ShouldSearchSubjects(SubjectMother.InClass(GuidMother.Random()), own);
        this.ShouldFindClass(classId);

        Result<IReadOnlyCollection<SubjectResponse>> result = await this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        result.Value.Select(response => response.Id).Should().Equal(own.Id);
    }

    [Test]
    public async Task return_an_empty_list_when_the_class_has_no_subjects()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchSubjects(SubjectMother.InClass(GuidMother.Random()));
        this.ShouldFindClass(classId);

        Result<IReadOnlyCollection<SubjectResponse>> result = await this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task return_class_not_found_without_searching_when_the_class_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        this.ShouldNotFindClass();

        Result<IReadOnlyCollection<SubjectResponse>> result = await this._useCase.ExecuteAsync(
            SubjectFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.ShouldNotHaveSearched();
    }
}
