namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.CalendarEvents.Application;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Application;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using System.Text;

[TestFixture]
public class ClassExpenseFinderByClassShould : ClassExpensesUnitTestCase
{
    private const int VALID_LIMIT = 10;

    private readonly ClassExpenseFinderByClass.Finder _useCase;

    public ClassExpenseFinderByClassShould()
    {
        this._useCase = new ClassExpenseFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_expense_of_the_class_with_its_event_author_and_participants_ordered_by_name()
    {
        Guid classId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid alphaId = GuidMother.Random();
        Guid betaId = GuidMother.Random();
        Guid gammaId = GuidMother.Random();
        string eventTitle = StringMother.Random("event");
        string authorName = StringMother.Random("author");
        string alphaName = StringMother.Random("alpha");
        string betaName = StringMother.Random("beta");
        string gammaName = StringMother.Random("gamma");
        ClassExpense expense = ClassExpenseMother.With(
            classId: classId,
            relatedEventId: eventId,
            authorId: authorId,
            participants:
            [
                ExpenseParticipantMother.Paid(gammaId),
                ExpenseParticipantMother.Unpaid(alphaId),
                ExpenseParticipantMother.Paid(betaId),
            ]);
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers(
            UserClassMemberResponseMother.Representative(authorId, authorName),
            UserClassMemberResponseMother.Participant(alphaId, alphaName),
            UserClassMemberResponseMother.Participant(betaId, betaName),
            UserClassMemberResponseMother.WithoutRole(gammaId, gammaName));
        this.ShouldFindCalendarEvents(CalendarEventSummaryResponseMother.With(eventId, eventTitle));
        this.ShouldSearchClassExpenses(expense, ClassExpenseMother.With(classId: GuidMother.Random()));

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        ClassExpenseResponse item = result.Value.Items.Should().ContainSingle().Subject;
        item.Id.Should().Be(expense.Id);
        item.ClassId.Should().Be(classId);
        item.Item.Should().Be(expense.Item);
        item.TotalAmount.Should().Be(expense.TotalAmount);
        item.RelatedEvent.Should().Be(CalendarEventSummaryResponseMother.With(eventId, eventTitle));
        item.AuthorId.Should().Be(authorId);
        item.AuthorName.Should().Be(authorName);
        item.AuthorRole.Should().Be("ClassRepresentative");
        item.CreatedAt.Should().Be(expense.CreatedAt);
        item.Participants.Should().Equal(
            new ExpenseParticipantResponse(alphaId, alphaName, false),
            new ExpenseParticipantResponse(betaId, betaName, true),
            new ExpenseParticipantResponse(gammaId, gammaName, true));
        result.Value.NextCursor.Should().BeNull();
        this.ShouldHaveAskedForClass(classId);
    }

    [Test]
    public async Task return_empty_names_and_no_role_and_no_event_when_they_are_not_found_or_there_is_none()
    {
        Guid classId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense withEvent = ClassExpenseMother.With(
            classId: classId,
            relatedEventId: GuidMother.Random(),
            createdAt: Now.AddMinutes(2),
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        ClassExpense withoutEvent = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(1));
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(withEvent, withoutEvent);

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(item => item.RelatedEvent == null);
        ClassExpenseResponse first = result.Value.Items.First();
        first.AuthorName.Should().BeEmpty();
        first.AuthorRole.Should().BeNull();
        first.Participants.Should().Equal(new ExpenseParticipantResponse(guardianId, string.Empty, false));
    }

    [Test]
    public async Task return_an_empty_page_without_next_cursor_when_the_class_has_no_expenses()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchNoClassExpenses();

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_the_newest_first_and_the_next_cursor_when_there_are_more_than_the_limit()
    {
        Guid classId = GuidMother.Random();
        ClassExpense minute1 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(1));
        ClassExpense minute2 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(2));
        ClassExpense minute3 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(3));
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(minute1, minute3, minute2);

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithLimit(classId, 2));

        result.Value.Items.Select(item => item.Id).Should().Equal(minute3.Id, minute2.Id);
        PageCursor.TryDecode(result.Value.NextCursor, out PageCursor cursor).Should().BeTrue();
        cursor.Key.Should().Be(minute2.CreatedAt.UtcTicks);
        cursor.Id.Should().Be(minute2.Id);
    }

    [Test]
    public async Task return_no_next_cursor_when_there_are_as_many_expenses_as_the_limit()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(1)),
            ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(2)));

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithLimit(classId, 2));

        result.Value.Items.Should().HaveCount(2);
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_only_what_comes_after_the_boundary_when_following_the_next_cursor()
    {
        Guid classId = GuidMother.Random();
        ClassExpense minute1 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(1));
        ClassExpense minute2 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(2));
        ClassExpense minute3 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(3));
        ClassExpense minute4 = ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(4));
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(minute2, minute4, minute1, minute3);
        Result<PagedResponse<ClassExpenseResponse>> firstPage = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithLimit(classId, 2));

        Result<PagedResponse<ClassExpenseResponse>> secondPage = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithCursor(classId, 2, firstPage.Value.NextCursor!));

        secondPage.Value.Items.Select(item => item.Id).Should().Equal(minute2.Id, minute1.Id);
        secondPage.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task paginate_expenses_created_at_the_same_time_without_duplicates_or_gaps()
    {
        Guid classId = GuidMother.Random();
        ClassExpense[] tied =
        [
            ClassExpenseMother.With(classId: classId),
            ClassExpenseMother.With(classId: classId),
            ClassExpenseMother.With(classId: classId),
        ];
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(tied);
        List<Guid> seen = [];
        string? cursor = null;

        for (int requested = 0; requested <= tied.Length; requested++)
        {
            Result<PagedResponse<ClassExpenseResponse>> page = await this._useCase.ExecuteAsync(
                ClassExpenseFinderByClassQueryMother.With(classId, 1, cursor));
            seen.AddRange(page.Value.Items.Select(item => item.Id));
            cursor = page.Value.NextCursor;
            if (cursor is null)
            {
                break;
            }
        }

        cursor.Should().BeNull();
        seen.Should().BeEquivalentTo(tied.Select(expense => expense.Id));
    }

    [Test]
    public async Task order_expenses_created_at_the_same_time_by_descending_id()
    {
        Guid classId = GuidMother.Random();
        ClassExpense low = ClassExpenseMother.With(
            id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            classId: classId);
        ClassExpense high = ClassExpenseMother.With(
            id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
            classId: classId);
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(low, high);

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        result.Value.Items.Select(item => item.Id).Should().Equal(high.Id, low.Id);
    }

    [Test]
    public async Task return_the_default_number_of_expenses_when_no_limit_is_given()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(Enumerable.Range(1, Pagination.DefaultLimit + 1)
            .Select(minutes => ClassExpenseMother.With(classId: classId, createdAt: Now.AddMinutes(minutes)))
            .ToArray());

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        result.Value.Items.Should().HaveCount(Pagination.DefaultLimit);
        result.Value.NextCursor.Should().NotBeNull();
    }

    [Test]
    public async Task ask_for_the_members_and_events_only_of_the_returned_page()
    {
        Guid classId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense newest = ClassExpenseMother.With(
            classId: classId,
            relatedEventId: eventId,
            authorId: authorId,
            createdAt: Now.AddMinutes(2),
            participants: [ExpenseParticipantMother.Unpaid(guardianId), ExpenseParticipantMother.Paid(authorId)]);
        ClassExpense cut = ClassExpenseMother.With(
            classId: classId,
            relatedEventId: GuidMother.Random(),
            createdAt: Now.AddMinutes(1));
        this.ShouldFindClass(classId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldSearchClassExpenses(cut, newest);

        await this._useCase.ExecuteAsync(ClassExpenseFinderByClassQueryMother.WithLimit(classId, 1));

        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserClassMemberFinderByIds.Query>(query => query.ClassId == classId
                    && query.UserIds.Count == 2
                    && query.UserIds.Contains(authorId)
                    && query.UserIds.Contains(guardianId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<CalendarEventFinderByIds.Query>(query => query.CalendarEventIds.Count == 1
                    && query.CalendarEventIds.Contains(eventId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task return_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.ClassAccessForbidden);
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_unauthenticated_without_reading_when_there_is_no_current_user()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldHaveNoCurrentUser();

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_without_searching_when_the_class_is_unknown()
    {
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(51)]
    public async Task return_invalid_limit_without_asking_for_the_class_when_the_limit_is_out_of_range(int limit)
    {
        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithLimit(GuidMother.Random(), limit));

        result.Error!.Code.Should().Be("Pagination.InvalidLimit");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("garbage!")]
    [TestCase("")]
    public async Task return_invalid_cursor_without_reading_anything_when_the_cursor_is_invalid(string cursor)
    {
        Guid classId = GuidMother.Random();

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor));

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_invalid_cursor_when_the_cursor_key_cannot_be_parsed()
    {
        Guid classId = GuidMother.Random();
        string cursor = Convert.ToBase64String(Encoding.UTF8.GetBytes("abc|" + GuidMother.Random())).TrimEnd('=');

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor));

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

        Result<PagedResponse<ClassExpenseResponse>> result = await this._useCase.ExecuteAsync(
            ClassExpenseFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor));

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    private void ShouldHaveClassMembers(params UserClassMemberResponse[] members)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserClassMemberFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<UserClassMemberResponse>)members
                    .Where(member => query.UserIds.Contains(member.UserId))
                    .ToList());

    private void ShouldFindCalendarEvents(params CalendarEventSummaryResponse[] calendarEvents)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<CalendarEventFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CalendarEventFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<CalendarEventSummaryResponse>)calendarEvents
                    .Where(calendarEvent => query.CalendarEventIds.Contains(calendarEvent.Id))
                    .ToList());
}
