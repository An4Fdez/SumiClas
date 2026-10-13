namespace SumiClas.Tests.Announcements;

using Moq;
using SumiClas.Announcements.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class AnnouncementsUnitTestCase : UnitTestCase
{
    protected Mock<IAnnouncementRepository> Repository { get; } = new();

    [SetUp]
    public void AnnouncementsSetUp()
    {
        this.Repository.Reset();
        this.ShouldReturn();
    }

    protected void ShouldReturn(params Announcement[] announcements)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Announcement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Announcement> specification, CancellationToken _)
                => InMemorySearch.Run(announcements, specification));

    protected void ShouldReturnInSequence(
        IReadOnlyCollection<Announcement> first,
        IReadOnlyCollection<Announcement> second)
        => this.Repository
            .SetupSequence(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Announcement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(first)
            .ReturnsAsync(second);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Announcement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
