namespace SumiClas.Tests.Subjects.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Subjects.Domain.Services;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Subjects.Domain;

[TestFixture]
public class SubjectFinderByIdShould : SubjectsUnitTestCase
{
    private readonly SubjectFinderById.Finder _useCase;

    public SubjectFinderByIdShould()
    {
        this._useCase = new SubjectFinderById.Finder(new SubjectByIdFinder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_subject_with_its_class_and_label_when_it_exists()
    {
        Guid classId = GuidMother.Random();
        Subject subject = SubjectMother.InClass(classId);
        this.ShouldFetchSubjects(SubjectMother.InClass(classId), subject);

        SubjectResponse result = await this._useCase.ExecuteAsync(
            SubjectFinderByIdQueryMother.WithSubjectId(subject.Id));
        result.Should().Be(SubjectResponseMother.With(subject.Id, classId, subject.Label));
    }

    [Test]
    public async Task throw_not_found_when_the_subject_does_not_exist()
    {
        this.ShouldFetchSubjects(SubjectMother.InClass(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectFinderByIdQueryMother.WithSubjectId(GuidMother.Random()));

        await act.Should().ThrowAsync<SubjectExceptions.NotFound>();
    }
}
