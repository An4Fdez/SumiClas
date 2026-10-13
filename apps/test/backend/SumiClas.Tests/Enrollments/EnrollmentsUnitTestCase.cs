namespace SumiClas.Tests.Enrollments;

using Moq;
using SumiClas.Enrollments.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class EnrollmentsUnitTestCase : UnitTestCase
{
    private readonly List<Enrollment> _stored = [];
    private readonly List<Enrollment> _added = [];

    protected Mock<IEnrollmentRepository> Repository { get; } = new();

    protected IReadOnlyList<Enrollment> Added => this._added;

    [SetUp]
    public void EnrollmentsSetUp()
    {
        this._stored.Clear();
        this._added.Clear();
        this.Repository.Reset();
        this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Enrollment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Enrollment> specification, CancellationToken _)
                => (IReadOnlyCollection<Enrollment>)this._stored.Where(specification.Criteria.Compile()).ToList());
        this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((enrollment, _) =>
            {
                this._added.Add(enrollment);
                this._stored.Add(enrollment);
            })
            .Returns(Task.CompletedTask);
    }

    protected void ShouldReturn(params Enrollment[] enrollments) => this._stored.AddRange(enrollments);
}
