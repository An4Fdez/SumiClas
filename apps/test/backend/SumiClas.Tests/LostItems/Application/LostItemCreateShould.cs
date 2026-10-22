namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class LostItemCreateShould : LostItemsUnitTestCase
{
    private readonly LostItemCreate.Create _useCase;

    public LostItemCreateShould()
    {
        this._useCase = new LostItemCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_school_item_trimmed_unrecovered_with_the_author_and_the_clock_time()
    {
        Guid itemId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string description = StringMother.Random("description");
        string photoUrl = StringMother.Random("photo");
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.With(
            itemId,
            "School",
            classId,
            "Found",
            "  " + description + " ",
            "  " + photoUrl + " ",
            true));

        result.IsSuccess.Should().BeTrue();
        LostItem added = this.Added.Single();
        added.Id.Should().Be(itemId);
        added.Scope.Should().Be(ItemScope.School);
        added.ClassId.Should().BeNull();
        added.SchoolId.Should().Be(schoolId);
        added.Status.Should().Be(ItemStatus.Found);
        added.Recovered.Should().BeFalse();
        added.Description.Should().Be(description);
        added.PhotoUrl.Should().Be(photoUrl);
        added.PhotoPrivacyConfirmed.Should().BeTrue();
        added.AuthorId.Should().Be(authorId);
        added.CreatedAt.Should().Be(Now);
    }

    [Test]
    public async Task add_the_class_item_with_its_class_when_the_class_exists_and_the_school_year_is_active()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "Class", classId: classId));

        result.IsSuccess.Should().BeTrue();
        LostItem added = this.Added.Single();
        added.Scope.Should().Be(ItemScope.Class);
        added.ClassId.Should().Be(classId);
    }

    [TestCase("SCHOOL", ItemScope.School)]
    [TestCase("class", ItemScope.Class)]
    [TestCase("0", ItemScope.School)]
    [TestCase("1", ItemScope.Class)]
    public async Task accept_the_scope_whatever_its_case_or_when_it_is_a_defined_number(
        string scope,
        ItemScope expected)
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: scope, classId: classId));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().Scope.Should().Be(expected);
    }

    [TestCase("LOST", ItemStatus.Lost)]
    [TestCase("found", ItemStatus.Found)]
    [TestCase("0", ItemStatus.Lost)]
    [TestCase("1", ItemStatus.Found)]
    public async Task accept_the_status_whatever_its_case_or_when_it_is_a_defined_number(
        string status,
        ItemStatus expected)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(classId: classId, status: status));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().Status.Should().Be(expected);
    }

    [TestCase(null)]
    [TestCase("   ")]
    public async Task store_no_photo_and_not_require_the_confirmation_when_the_photo_is_null_or_blank(
        string? photoUrl)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.With(
            GuidMother.Random(),
            "Class",
            classId,
            "Lost",
            StringMother.Random("item"),
            photoUrl,
            false));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().PhotoUrl.Should().BeNull();
    }

    [Test]
    public async Task not_require_the_author_to_be_the_class_representative()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedLostItems();

        await this._useCase.ExecuteAsync(LostItemCreateCommandMother.Valid(scope: "Class", classId: classId));

        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserIsClassRepresentative.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();
        this.ShouldCaptureAddedLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "garbage", status: "garbage"));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("garbage")]
    [TestCase("")]
    [TestCase("2")]
    [TestCase("-1")]
    public async Task return_invalid_scope_without_asking_or_adding_when_the_scope_is_not_valid(string scope)
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(id: Guid.Empty, scope: scope, status: "garbage"));

        result.Error.Should().Be(LostItemErrors.InvalidScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("garbage")]
    [TestCase("")]
    [TestCase("2")]
    [TestCase("-1")]
    public async Task return_invalid_status_without_asking_or_adding_when_the_status_is_not_valid(string status)
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(id: Guid.Empty, status: status));

        result.Error.Should().Be(LostItemErrors.InvalidStatus);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_id_is_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.Valid(id: Guid.Empty));

        result.Error.Should().Be(LostItemErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_scope_has_no_class()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.With(
            GuidMother.Random(),
            "Class",
            null,
            "Lost",
            StringMother.Random("item"),
            null,
            false));

        result.Error.Should().Be(LostItemErrors.ClassRequiredForClassScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_when_the_class_scope_has_an_empty_class()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "Class", classId: Guid.Empty));

        result.Error.Should().Be(LostItemErrors.ClassRequiredForClassScope);
    }

    [Test]
    public async Task return_description_required_without_asking_or_adding_when_the_description_is_blank()
    {
        string description = "   ";
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(description: description));

        result.Error.Should().Be(LostItemErrors.DescriptionRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_photo_confirmation_required_without_asking_or_adding_when_it_is_not_confirmed()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.WithPhoto(StringMother.Random("photo"), false));

        result.Error.Should().Be(LostItemErrors.PhotoPrivacyConfirmationRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_author_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldCaptureAddedLostItems();

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.Valid());

        result.Error.Should().Be(LostItemErrors.AuthorRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_invalid_scope_when_the_scope_and_the_status_are_not_valid()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "garbage", status: "garbage"));

        result.Error.Should().Be(LostItemErrors.InvalidScope);
    }

    [Test]
    public async Task return_invalid_status_when_the_status_is_not_valid_and_the_id_is_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(id: Guid.Empty, status: "garbage"));

        result.Error.Should().Be(LostItemErrors.InvalidStatus);
    }

    [Test]
    public async Task return_id_required_when_the_id_is_empty_and_the_class_scope_has_no_class()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(id: Guid.Empty, scope: "Class", classId: Guid.Empty));

        result.Error.Should().Be(LostItemErrors.IdRequired);
    }

    [Test]
    public async Task return_class_required_when_the_class_is_empty_and_the_description_is_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "Class", classId: Guid.Empty, description: "  "));

        result.Error.Should().Be(LostItemErrors.ClassRequiredForClassScope);
    }

    [Test]
    public async Task return_description_required_when_the_description_is_blank_and_the_photo_is_not_confirmed()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.With(
            GuidMother.Random(),
            "School",
            null,
            "Lost",
            "  ",
            StringMother.Random("photo"),
            false));

        result.Error.Should().Be(LostItemErrors.DescriptionRequired);
    }

    [Test]
    public async Task return_photo_confirmation_required_when_it_is_not_confirmed_and_the_author_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.WithPhoto(StringMother.Random("photo"), false));

        result.Error.Should().Be(LostItemErrors.PhotoPrivacyConfirmationRequired);
    }

    [Test]
    public async Task throw_user_not_found_without_asking_for_the_class_when_the_author_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);
        this.ShouldCaptureAddedLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemCreateCommandMother.Valid(scope: "Class"));

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();
        this.ShouldCaptureAddedLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemCreateCommandMother.Valid(scope: "Class"));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_without_adding_when_the_school_year_is_archived()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldCaptureAddedLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "Class", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldCaptureAddedLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "Class", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_adding_when_the_author_does_not_belong_to_the_class()
    {
        this.ShouldCaptureAddedLostItems();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassAccess(authorId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "Class", classId: classId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_school_scope_has_no_class()
    {
        this.ShouldCaptureAddedLostItems();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(LostItemCreateCommandMother.With(
            GuidMother.Random(),
            "School",
            null,
            "Lost",
            StringMother.Random("item"),
            null,
            false));

        result.Error.Should().Be(LostItemErrors.ClassRequiredForSchoolScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_school_scope_has_an_empty_class()
    {
        this.ShouldCaptureAddedLostItems();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "School", classId: Guid.Empty));

        result.Error.Should().Be(LostItemErrors.ClassRequiredForSchoolScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedLostItems();
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "School", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_adding_the_school_item_when_the_author_is_not_in_the_class()
    {
        this.ShouldCaptureAddedLostItems();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassAccess(authorId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "School", classId: classId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_archived_without_adding_the_school_item_when_the_class_is_archived()
    {
        this.ShouldCaptureAddedLostItems();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemCreateCommandMother.Valid(scope: "School", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }
}
