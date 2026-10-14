namespace SumiClas.Tests.Assignments.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Assignments.Application;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Application;
using SumiClas.Tests.Subjects.Application;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Application;
using System.Text;

[TestFixture]
public class AssignmentFinderByClassShould : AssignmentsUnitTestCase
{
    private const int VALID_LIMIT = 10;

    private readonly AssignmentFinderByClass.Finder _useCase;

    public AssignmentFinderByClassShould()
    {
        this._useCase = new AssignmentFinderByClass.Finder(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task return_the_assignment_of_the_class_with_the_subject_the_author_and_the_comments_by_time()
    {
        Guid classId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid commenterId = GuidMother.Random();
        string subjectLabel = StringMother.Random("subject");
        string authorName = StringMother.Random("author");
        string commenterName = StringMother.Random("commenter");
        string firstText = StringMother.Random("first");
        string secondText = StringMother.Random("second");
        Assignment assignment = AssignmentMother.With(
            classId: classId,
            subjectId: subjectId,
            authorId: authorId,
            isProposal: true,
            description: StringMother.Random("description"),
            comments:
            [
                AssignmentCommentMother.With(commenterId, secondText, Now.AddMinutes(2)),
                AssignmentCommentMother.With(authorId, firstText, Now.AddMinutes(1)),
            ]);
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId, SubjectResponseMother.With(subjectId, classId, subjectLabel));
        this.ShouldHaveClassMembers(
            UserClassMemberResponseMother.Participant(authorId, authorName),
            UserClassMemberResponseMother.Representative(commenterId, commenterName));
        this.ShouldSearchAssignments(assignment, AssignmentMother.With(classId: GuidMother.Random()));

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(classId));

        AssignmentResponse item = result.Value.Items.Should().ContainSingle().Subject;
        item.Id.Should().Be(assignment.Id);
        item.ClassId.Should().Be(classId);
        item.SubjectId.Should().Be(subjectId);
        item.SubjectLabel.Should().Be(subjectLabel);
        item.Title.Should().Be(assignment.Title);
        item.Description.Should().Be(assignment.Description);
        item.DueDate.Should().Be(assignment.DueDate);
        item.AuthorId.Should().Be(authorId);
        item.AuthorName.Should().Be(authorName);
        item.AuthorRole.Should().Be("Participant");
        item.IsProposal.Should().BeTrue();
        item.CreatedAt.Should().Be(assignment.CreatedAt);
        item.Comments.Select(comment => comment.Text).Should().Equal(firstText, secondText);
        item.Comments.Select(comment => comment.AuthorId).Should().Equal(authorId, commenterId);
        item.Comments.Select(comment => comment.AuthorName).Should().Equal(authorName, commenterName);
        item.Comments.Select(comment => comment.AuthorRole).Should().Equal("Participant", "ClassRepresentative");
        item.Comments.Select(comment => comment.CreatedAt).Should().Equal(Now.AddMinutes(1), Now.AddMinutes(2));
        item.Comments.Select(comment => comment.Id)
            .Should().Equal(assignment.Comments.OrderBy(comment => comment.CreatedAt).Select(comment => comment.Id));
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_empty_subject_label_and_author_when_they_are_not_found()
    {
        Guid classId = GuidMother.Random();
        Guid commenterId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(
            classId: classId,
            comments: [AssignmentCommentMother.With(commenterId, StringMother.Random("text"), Now)]);
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(assignment);

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(classId));

        AssignmentResponse item = result.Value.Items.Should().ContainSingle().Subject;
        item.SubjectLabel.Should().BeEmpty();
        item.AuthorName.Should().BeEmpty();
        item.AuthorRole.Should().BeNull();
        AssignmentCommentResponse comment = item.Comments.Should().ContainSingle().Subject;
        comment.AuthorName.Should().BeEmpty();
        comment.AuthorRole.Should().BeNull();
    }

    [Test]
    public async Task return_an_empty_page_without_next_cursor_when_the_class_has_no_assignments()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchNoAssignments();

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_the_earliest_due_first_and_the_next_cursor_when_there_are_more_than_the_limit()
    {
        Guid classId = GuidMother.Random();
        Assignment day1 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(1));
        Assignment day2 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(2));
        Assignment day3 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(3));
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(day3, day1, day2);

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithLimit(classId, 2));

        result.Value.Items.Select(item => item.Id).Should().Equal(day1.Id, day2.Id);
        PageCursor.TryDecode(result.Value.NextCursor, out PageCursor cursor).Should().BeTrue();
        cursor.Key.Should().Be(day2.DueDate.DayNumber);
        cursor.Id.Should().Be(day2.Id);
    }

    [Test]
    public async Task return_no_next_cursor_when_there_are_as_many_assignments_as_the_limit()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(1)),
            AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(2)));

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithLimit(classId, 2));

        result.Value.Items.Should().HaveCount(2);
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_only_what_comes_after_the_boundary_when_following_the_next_cursor()
    {
        Guid classId = GuidMother.Random();
        Assignment day1 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(1));
        Assignment day2 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(2));
        Assignment day3 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(3));
        Assignment day4 = AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(4));
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(day4, day2, day1, day3);
        Result<PagedResponse<AssignmentResponse>> firstPage = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithLimit(classId, 2));

        Result<PagedResponse<AssignmentResponse>> secondPage = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithCursor(classId, 2, firstPage.Value.NextCursor!));

        secondPage.Value.Items.Select(item => item.Id).Should().Equal(day3.Id, day4.Id);
        secondPage.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task paginate_assignments_due_the_same_day_without_duplicates_or_gaps()
    {
        Guid classId = GuidMother.Random();
        Assignment[] tied =
        [
            AssignmentMother.With(classId: classId),
            AssignmentMother.With(classId: classId),
            AssignmentMother.With(classId: classId),
        ];
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(tied);
        List<Guid> seen = [];
        string? cursor = null;

        for (int requested = 0; requested <= tied.Length; requested++)
        {
            Result<PagedResponse<AssignmentResponse>> page = await this._useCase.ExecuteAsync(
                AssignmentFinderByClassQueryMother.With(classId, 1, cursor));
            seen.AddRange(page.Value.Items.Select(item => item.Id));
            cursor = page.Value.NextCursor;
            if (cursor is null)
            {
                break;
            }
        }

        cursor.Should().BeNull();
        seen.Should().BeEquivalentTo(tied.Select(assignment => assignment.Id));
    }

    [Test]
    public async Task order_assignments_due_the_same_day_by_ascending_id()
    {
        Guid classId = GuidMother.Random();
        Assignment low = AssignmentMother.With(
            id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            classId: classId);
        Assignment high = AssignmentMother.With(
            id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
            classId: classId);
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(high, low);

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(classId));

        result.Value.Items.Select(item => item.Id).Should().Equal(low.Id, high.Id);
    }

    [Test]
    public async Task return_the_default_number_of_assignments_when_no_limit_is_given()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(Enumerable.Range(1, Pagination.DefaultLimit + 1)
            .Select(days => AssignmentMother.With(classId: classId, dueDate: AssignmentMother.SomeDay.AddDays(days)))
            .ToArray());

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(classId));

        result.Value.Items.Should().HaveCount(Pagination.DefaultLimit);
        result.Value.NextCursor.Should().NotBeNull();
    }

    [Test]
    public async Task ask_for_the_members_only_for_the_authors_and_commenters_of_the_returned_page()
    {
        Guid classId = GuidMother.Random();
        Guid firstAuthorId = GuidMother.Random();
        Guid commenterId = GuidMother.Random();
        Guid cutAuthorId = GuidMother.Random();
        Assignment first = AssignmentMother.With(
            classId: classId,
            authorId: firstAuthorId,
            dueDate: AssignmentMother.SomeDay.AddDays(1),
            comments:
            [
                AssignmentCommentMother.With(commenterId, StringMother.Random("text"), Now),
                AssignmentCommentMother.With(firstAuthorId, StringMother.Random("text"), Now),
                AssignmentCommentMother.With(commenterId, StringMother.Random("text"), Now),
            ]);
        Assignment cut = AssignmentMother.With(
            classId: classId,
            authorId: cutAuthorId,
            dueDate: AssignmentMother.SomeDay.AddDays(2));
        this.ShouldFindClass(classId);
        this.ShouldFindSubjectsOfClass(classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchAssignments(cut, first);

        await this._useCase.ExecuteAsync(AssignmentFinderByClassQueryMother.WithLimit(classId, 1));

        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserClassMemberFinderByIds.Query>(query => query.ClassId == classId
                    && query.UserIds.Count == 2
                    && query.UserIds.Contains(firstAuthorId)
                    && query.UserIds.Contains(commenterId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task return_the_subjects_error_when_the_subjects_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldFailFindingSubjectsOfClass(ClassErrors.NotFound);
        this.ShouldSearchAssignments(AssignmentMother.With(classId: classId));

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(ClassErrors.NotFound);
    }

    [Test]
    public async Task return_the_class_error_when_the_class_is_unknown()
    {
        this.ShouldNotFindClass();

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.ShouldNotHaveSearched();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(51)]
    public async Task return_invalid_limit_without_asking_for_the_class_when_the_limit_is_out_of_range(int limit)
    {
        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithLimit(GuidMother.Random(), limit));

        result.Error!.Code.Should().Be("Pagination.InvalidLimit");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("garbage!")]
    [TestCase("")]
    public async Task return_invalid_cursor_without_reading_anything_when_the_cursor_is_invalid(string cursor)
    {
        Guid classId = GuidMother.Random();

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor));

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_invalid_cursor_when_the_cursor_key_cannot_be_parsed()
    {
        Guid classId = GuidMother.Random();
        string cursor = Convert.ToBase64String(Encoding.UTF8.GetBytes("abc|" + GuidMother.Random())).TrimEnd('=');

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor));

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase(long.MaxValue)]
    [TestCase(-1)]
    public async Task return_invalid_cursor_when_the_cursor_key_is_out_of_range_for_a_date(long key)
    {
        Guid classId = GuidMother.Random();
        string cursor = PageCursorMother.Create(key, GuidMother.Random()).Encode();

        Result<PagedResponse<AssignmentResponse>> result = await this._useCase.ExecuteAsync(
            AssignmentFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor));

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    private void ShouldFindSubjectsOfClass(Guid classId, params SubjectResponse[] subjects)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SubjectFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<SubjectResponse>>.Success(subjects));

    private void ShouldFailFindingSubjectsOfClass(Error error)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<SubjectFinderByClass.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<SubjectResponse>>.Failure(error));

    private void ShouldHaveClassMembers(params UserClassMemberResponse[] members)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserClassMemberFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<UserClassMemberResponse>)members
                    .Where(member => query.UserIds.Contains(member.UserId))
                    .ToList());
}
