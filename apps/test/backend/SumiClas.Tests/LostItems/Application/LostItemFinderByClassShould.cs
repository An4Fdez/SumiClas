namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class LostItemFinderByClassShould : LostItemsUnitTestCase
{
    private const int VALID_LIMIT = 10;

    private readonly LostItemFinderByClass.Finder _useCase;

    public LostItemFinderByClassShould()
    {
        this._useCase = new LostItemFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_item_with_its_author_name_and_role()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string authorName = StringMother.Random("author");
        string description = StringMother.Random("description");
        string photoUrl = StringMother.Random("photo");
        LostItem lostItem = LostItemMother.With(schoolId: schoolId, 
            scope: ItemScope.Class,
            classId: classId,
            status: ItemStatus.Found,
            recovered: true,
            description: description,
            photoUrl: photoUrl,
            authorId: authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers(UserClassMemberResponseMother.Representative(authorId, authorName));
        this.ShouldSearchLostItems(lostItem);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        LostItemResponse item = result.Items.Should().ContainSingle().Subject;
        item.Id.Should().Be(lostItem.Id);
        item.Scope.Should().Be("Class");
        item.ClassId.Should().Be(classId);
        item.Status.Should().Be("Found");
        item.Recovered.Should().BeTrue();
        item.Description.Should().Be(description);
        item.PhotoUrl.Should().Be(photoUrl);
        item.AuthorId.Should().Be(authorId);
        item.AuthorName.Should().Be(authorName);
        item.AuthorRole.Should().Be("ClassRepresentative");
        item.CreatedAt.Should().Be(lostItem.CreatedAt);
        result.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_the_school_item_without_class_nor_photo_and_with_the_participant_role()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers(
            UserClassMemberResponseMother.Participant(authorId, StringMother.Random("author")));
        this.ShouldSearchLostItems(LostItemMother.With(schoolId: schoolId, authorId: authorId));

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        LostItemResponse item = result.Items.Should().ContainSingle().Subject;
        item.Scope.Should().Be("School");
        item.ClassId.Should().BeNull();
        item.Status.Should().Be("Lost");
        item.Recovered.Should().BeFalse();
        item.PhotoUrl.Should().BeNull();
        item.AuthorRole.Should().Be("Participant");
    }

    [Test]
    public async Task return_an_empty_name_and_no_role_when_the_author_is_not_a_member_of_the_class()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(LostItemMother.ForClass(classId));

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        LostItemResponse item = result.Items.Should().ContainSingle().Subject;
        item.AuthorName.Should().BeEmpty();
        item.AuthorRole.Should().BeNull();
    }

    [Test]
    public async Task return_the_school_items_and_the_items_of_the_class_but_not_those_of_other_classes()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        LostItem school = LostItemMother.With(schoolId: schoolId);
        LostItem mine = LostItemMother.ForClass(classId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(school, mine, LostItemMother.ForClass(GuidMother.Random()));

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Select(item => item.Id).Should().BeEquivalentTo([school.Id, mine.Id]);
    }

    [Test]
    public async Task return_an_empty_page_without_next_cursor_when_there_are_no_items()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchNoLostItems();

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_the_school_and_class_items_interleaved_newest_first()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        LostItem schoolOld = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(1));
        LostItem classMiddle = LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(2));
        LostItem schoolNew = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(3));
        LostItem classNewest = LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(4));
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(schoolOld, classNewest, schoolNew, classMiddle);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Select(item => item.Id)
            .Should().Equal(classNewest.Id, schoolNew.Id, classMiddle.Id, schoolOld.Id);
    }

    [Test]
    public async Task return_the_newest_first_and_the_next_cursor_when_there_are_more_than_the_limit()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        LostItem minute1 = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(1));
        LostItem minute2 = LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(2));
        LostItem minute3 = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(3));
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(minute1, minute3, minute2);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithLimit(classId, 2));

        result.Items.Select(item => item.Id).Should().Equal(minute3.Id, minute2.Id);
        PageCursor.TryDecode(result.NextCursor, out PageCursor cursor).Should().BeTrue();
        cursor.Key.Should().Be(minute2.CreatedAt.UtcTicks);
        cursor.Id.Should().Be(minute2.Id);
    }

    [Test]
    public async Task return_no_next_cursor_when_there_are_as_many_items_as_the_limit()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(
            LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(1)),
            LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(2)));

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithLimit(classId, 2));

        result.Items.Should().HaveCount(2);
        result.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_only_what_comes_after_the_boundary_when_following_the_next_cursor()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        LostItem minute1 = LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(1));
        LostItem minute2 = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(2));
        LostItem minute3 = LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(3));
        LostItem minute4 = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(4));
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(minute2, minute4, minute1, minute3);
        PagedResponse<LostItemResponse> firstPage = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithLimit(classId, 2));

        PagedResponse<LostItemResponse> secondPage = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithCursor(classId, 2, firstPage.NextCursor!));

        secondPage.Items.Select(item => item.Id).Should().Equal(minute2.Id, minute1.Id);
        secondPage.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task paginate_items_created_at_the_same_time_without_duplicates_or_gaps()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        LostItem[] tied =
        [
            LostItemMother.With(schoolId: schoolId),
            LostItemMother.ForClass(classId),
            LostItemMother.With(schoolId: schoolId),
            LostItemMother.ForClass(classId),
        ];
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(tied);
        List<Guid> seen = [];
        string? cursor = null;

        for (int requested = 0; requested <= tied.Length; requested++)
        {
            PagedResponse<LostItemResponse> page = await this._useCase.ExecuteAsync(
                LostItemFinderByClassQueryMother.With(classId, 1, cursor));
            seen.AddRange(page.Items.Select(item => item.Id));
            cursor = page.NextCursor;
            if (cursor is null)
            {
                break;
            }
        }

        cursor.Should().BeNull();
        seen.Should().BeEquivalentTo(tied.Select(lostItem => lostItem.Id));
    }

    [Test]
    public async Task order_items_created_at_the_same_time_by_descending_id()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        LostItem low = LostItemMother.With(schoolId: schoolId, id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        LostItem high = LostItemMother.ForClass(classId, id: Guid.Parse("ff000000-0000-0000-0000-000000000000"));
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(low, high);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Select(item => item.Id).Should().Equal(high.Id, low.Id);
    }

    [Test]
    public async Task return_the_default_number_of_items_when_no_limit_is_given()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(Enumerable.Range(1, Pagination.DefaultLimit + 1)
            .Select(minutes => LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(minutes)))
            .ToArray());

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Should().HaveCount(Pagination.DefaultLimit);
        result.NextCursor.Should().NotBeNull();
    }

    [Test]
    public async Task ask_for_the_members_only_of_the_returned_page_and_once_per_author()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        LostItem first = LostItemMother.With(schoolId: schoolId, authorId: authorId, createdAt: Now.AddMinutes(4));
        LostItem sameAuthor = LostItemMother.With(schoolId: schoolId, authorId: authorId, createdAt: Now.AddMinutes(3));
        LostItem second = LostItemMother.ForClass(classId, createdAt: Now.AddMinutes(2));
        Guid secondAuthorId = second.AuthorId;
        LostItem cut = LostItemMother.With(schoolId: schoolId, createdAt: Now.AddMinutes(1));
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(first, sameAuthor, second, cut);

        await this._useCase.ExecuteAsync(LostItemFinderByClassQueryMother.WithLimit(classId, 3));

        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserClassMemberFinderByIds.Query>(query => query.ClassId == classId
                    && query.UserIds.Count == 2
                    && query.UserIds.Contains(authorId)
                    && query.UserIds.Contains(secondAuthorId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
            LostItemFinderByClassQueryMother.WithClassId(classId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_unauthenticated_before_validating_the_limit_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithLimit(GuidMother.Random(), Pagination.MaxLimit + 1));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task not_return_the_school_items_of_other_schools()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        LostItem mine = LostItemMother.With(schoolId: schoolId);
        LostItem otherSchool = LostItemMother.With(schoolId: GuidMother.Random());
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(mine, otherSchool);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Select(item => item.Id).Should().Equal(mine.Id);
    }

    [Test]
    public async Task throw_the_school_error_without_searching_when_the_school_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_without_searching_when_the_class_is_unknown()
    {
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(51)]
    public async Task throw_invalid_limit_without_asking_for_the_class_when_the_limit_is_out_of_range(int limit)
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithLimit(GuidMother.Random(), limit));

        await act.Should().ThrowAsync<PaginationExceptions.InvalidLimit>();
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
    }

    [TestCase("garbage!")]
    [TestCase("")]
    public async Task throw_invalid_cursor_without_asking_for_the_class_when_the_cursor_is_invalid(string cursor)
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithCursor(GuidMother.Random(), VALID_LIMIT, cursor));

        await act.Should().ThrowAsync<PaginationExceptions.InvalidCursor>();
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_the_pending_claims_count_to_the_author_and_no_claim_status()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        LostItemClaim[] pending = [LostItemClaimMother.With(), LostItemClaimMother.With()];
        LostItem lostItem = LostItemMother.ForClass(
            classId,
            authorId: currentUserId,
            claims: [.. pending, LostItemClaimMother.With(status: ClaimStatus.Rejected)]);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(lostItem);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        LostItemResponse item = result.Items.Should().ContainSingle().Subject;
        item.PendingClaimsCount.Should().Be(pending.Length);
        item.MyClaimStatus.Should().BeNull();
    }

    [Test]
    public async Task return_the_status_of_the_latest_own_claim_to_a_member_and_no_count()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        LostItem withClaim = LostItemMother.ForClass(
            classId,
            claims:
            [
                LostItemClaimMother.With(claimantId: currentUserId, status: ClaimStatus.Rejected),
                LostItemClaimMother.PendingOf(GuidMother.Random()),
            ]);
        LostItem withoutClaim = LostItemMother.ForClass(classId, claims: [LostItemClaimMother.PendingOf(GuidMother.Random())]);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldHaveClassMembers();
        this.ShouldSearchLostItems(withClaim, withoutClaim);

        PagedResponse<LostItemResponse> result = await this._useCase.ExecuteAsync(
            LostItemFinderByClassQueryMother.WithClassId(classId));

        result.Items.Should().OnlyContain(item => item.PendingClaimsCount == null);
        result.Items.Single(item => item.Id == withClaim.Id).MyClaimStatus.Should().Be("Rejected");
        result.Items.Single(item => item.Id == withoutClaim.Id).MyClaimStatus.Should().BeNull();
    }

    private void ShouldHaveClassMembers(params UserClassMemberResponse[] members)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserClassMemberFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<UserClassMemberResponse>)members
                    .Where(member => query.UserIds.Contains(member.UserId))
                    .ToList());
}
