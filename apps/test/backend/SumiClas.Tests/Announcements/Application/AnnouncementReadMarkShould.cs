namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class AnnouncementReadMarkShould : AnnouncementsUnitTestCase
{
    private readonly AnnouncementReadMark.Mark _useCase;

    public AnnouncementReadMarkShould()
    {
        this._useCase = new AnnouncementReadMark.Mark(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_a_receipt_for_the_current_user_with_the_clock_time()
    {
        Guid announcementId = GuidMother.Random();
        Guid readerId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.WithId(announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(readerId);
        this.ShouldFindUser(readerId);

        Result result = await this._useCase.ExecuteAsync(
            AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        result.IsSuccess.Should().BeTrue();
        AnnouncementReadReceipt receipt = announcement.ReadReceipts.Should().ContainSingle().Subject;
        receipt.UserId.Should().Be(readerId);
        receipt.ReadAt.Should().Be(Now);
    }

    [Test]
    public async Task keep_the_original_receipt_when_the_same_user_marks_it_again()
    {
        Guid announcementId = GuidMother.Random();
        Guid readerId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.WithId(announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(readerId);
        this.ShouldFindUser(readerId);
        await this._useCase.ExecuteAsync(AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));
        this.AdvanceClock(TimeSpan.FromHours(2));

        Result result = await this._useCase.ExecuteAsync(
            AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        result.IsSuccess.Should().BeTrue();
        AnnouncementReadReceipt receipt = announcement.ReadReceipts.Should().ContainSingle().Subject;
        receipt.UserId.Should().Be(readerId);
        receipt.ReadAt.Should().Be(Now);
    }

    [Test]
    public async Task keep_one_receipt_per_user_when_several_users_mark_it()
    {
        Guid announcementId = GuidMother.Random();
        Guid firstReaderId = GuidMother.Random();
        Guid secondReaderId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.WithId(announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(firstReaderId);
        this.ShouldFindUser(firstReaderId);
        await this._useCase.ExecuteAsync(AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));
        this.AdvanceClock(TimeSpan.FromHours(2));
        this.ShouldResolveCurrentUser(secondReaderId);
        this.ShouldFindUser(secondReaderId);

        await this._useCase.ExecuteAsync(AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        announcement.ReadReceipts.Select(receipt => receipt.UserId)
            .Should().BeEquivalentTo(new[] { firstReaderId, secondReaderId });
        announcement.ReadReceipts.Single(receipt => receipt.UserId == secondReaderId).ReadAt
            .Should().Be(Now.AddHours(2));
        announcement.ReadReceipts.Single(receipt => receipt.UserId == firstReaderId).ReadAt
            .Should().Be(Now);
    }

    [Test]
    public async Task mark_only_the_requested_announcement()
    {
        Guid announcementId = GuidMother.Random();
        Announcement requested = AnnouncementMother.WithId(announcementId);
        Announcement other = AnnouncementMother.WithId(GuidMother.Random());
        this.ShouldFetchAnnouncements(other, requested);
        Guid readerId = GuidMother.Random();
        this.ShouldResolveCurrentUser(readerId);
        this.ShouldFindUser(readerId);

        await this._useCase.ExecuteAsync(AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        requested.ReadReceipts.Should().HaveCount(1);
        other.ReadReceipts.Should().BeEmpty();
    }

    [Test]
    public async Task not_add_any_announcement_to_the_repository()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid announcementId = GuidMother.Random();
        this.ShouldFetchAnnouncements(AnnouncementMother.WithId(announcementId));
        Guid readerId = GuidMother.Random();
        this.ShouldResolveCurrentUser(readerId);
        this.ShouldFindUser(readerId);

        await this._useCase.ExecuteAsync(AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_unauthenticated_without_reading_the_announcement_or_asking_when_there_is_no_current_user()
    {
        Guid announcementId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.WithId(announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        announcement.ReadReceipts.Should().BeEmpty();
        this.ShouldNotHaveRead();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_not_found_without_asking_for_the_reader_when_the_announcement_does_not_exist()
    {
        Guid readerId = GuidMother.Random();
        this.ShouldFetchAnnouncements(AnnouncementMother.WithId(GuidMother.Random()));
        this.ShouldResolveCurrentUser(readerId);

        Result result = await this._useCase.ExecuteAsync(
            AnnouncementReadMarkCommandMother.WithAnnouncementId(GuidMother.Random()));

        result.Error.Should().Be(AnnouncementErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_user_not_found_without_adding_a_receipt_when_the_reader_does_not_exist()
    {
        Guid announcementId = GuidMother.Random();
        Guid readerId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.WithId(announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(readerId);
        this.ShouldNotFindUser(readerId);

        Result result = await this._useCase.ExecuteAsync(
            AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        result.Error.Should().Be(UserErrors.NotFound);
        announcement.ReadReceipts.Should().BeEmpty();
    }

    [Test]
    public async Task return_receipt_user_required_without_adding_a_receipt_when_the_current_user_id_is_empty()
    {
        Guid announcementId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.WithId(announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldFindUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(
            AnnouncementReadMarkCommandMother.WithAnnouncementId(announcementId));

        result.Error.Should().Be(AnnouncementErrors.ReadReceiptUserRequired);
        announcement.ReadReceipts.Should().BeEmpty();
    }
}
