namespace SumiClas.Tests.WeeklySummaries;

using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Assignments.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.LostItems.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Application;
using SumiClas.WeeklySummaries.Domain;

public abstract class WeeklySummariesUnitTestCase : UnitTestCase
{
    private readonly List<WeeklySummary> _added = [];
    private readonly List<WeeklySummaryInput> _writtenFrom = [];

    protected Mock<IWeeklySummaryRepository> Repository { get; } = new();

    protected Mock<IWeeklySummaryWriter> Writer { get; } = new();

    protected IReadOnlyList<WeeklySummary> Added => this._added;

    protected IReadOnlyList<WeeklySummaryInput> WrittenFrom => this._writtenFrom;

    [SetUp]
    public void WeeklySummariesSetUp()
    {
        this._added.Clear();
        this._writtenFrom.Clear();
        this.Repository.Reset();
        this.Writer.Reset();
    }

    protected void ShouldCaptureAddedWeeklySummaries()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<WeeklySummary>(), It.IsAny<CancellationToken>()))
            .Callback<WeeklySummary, CancellationToken>((weeklySummary, _) => this._added.Add(weeklySummary))
            .Returns(Task.CompletedTask);

    protected void ShouldSearchWeeklySummaries(params WeeklySummary[] weeklySummaries)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<WeeklySummary>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<WeeklySummary> specification, CancellationToken _)
                => InMemorySearch.Run(weeklySummaries, specification));

    protected void ShouldWrite(string content)
        => this.Writer
            .Setup(writer => writer.WriteAsync(It.IsAny<WeeklySummaryInput>(), It.IsAny<CancellationToken>()))
            .Callback<WeeklySummaryInput, CancellationToken>((input, _) => this._writtenFrom.Add(input))
            .ReturnsAsync(content);

    protected void ShouldFailWriting(Exception exception)
        => this.Writer
            .Setup(writer => writer.WriteAsync(It.IsAny<WeeklySummaryInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    protected void ShouldNotHaveWritten()
        => this.Writer.Verify(
            writer => writer.WriteAsync(It.IsAny<WeeklySummaryInput>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldReadWeek(
        Guid classId,
        DateOnly from,
        DateOnly to,
        IReadOnlyCollection<AnnouncementPeriodResponse>? announcements = null,
        IReadOnlyCollection<CalendarEventPeriodResponse>? events = null,
        IReadOnlyCollection<AssignmentPeriodResponse>? assignments = null,
        IReadOnlyCollection<ClassExpenseOpenResponse>? expenses = null,
        int lostItems = 0)
    {
        this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<AnnouncementPeriodFinderByClass.Query>(
                    query => query.ClassId == classId && query.From == from && query.To == to),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(announcements ?? []);
        this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventPeriodFinderByClass.Query>(
                    query => query.ClassId == classId && query.From == from && query.To == to),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events ?? []);
        this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<AssignmentPeriodFinderByClass.Query>(
                    query => query.ClassId == classId && query.From == from && query.To == to),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments ?? []);
        this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassExpenseOpenFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses ?? []);
        this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<LostItemOpenFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LostItemOpenResponse(lostItems));
    }

    protected void ShouldFailReadingWeek(Exception exception)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<AnnouncementPeriodFinderByClass.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<WeeklySummary>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
