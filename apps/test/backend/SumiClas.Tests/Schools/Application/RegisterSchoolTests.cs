using Moq;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Schools.Application;

[TestFixture]
public class RegisterSchoolTests
{
    private static readonly Guid SchoolId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");

    private Mock<ISchoolRepository> _schools = null!;
    private List<School> _added = null!;
    private RegisterSchool.UseCase _useCase = null!;
    private RegisterSchool.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _added = [];
        _schools = new Mock<ISchoolRepository>();
        _schools
            .Setup(repository => repository.AddAsync(It.IsAny<School>(), It.IsAny<CancellationToken>()))
            .Callback<School, CancellationToken>((school, _) => _added.Add(school))
            .Returns(Task.CompletedTask);

        _useCase = new RegisterSchool.UseCase(_schools.Object);
        _handler = new RegisterSchool.Handler(_useCase);
    }

    [Test]
    public async Task UseCase_WithValidCommand_ReturnsSuccessAndAddsSchool()
    {
        var command = new RegisterSchool.Command(SchoolId, "  CEIP Sumi  ", "  28000001  ", "Calle Mayor 1");

        var result = await _useCase.ExecuteAsync(command);

        Assert.That(result.IsSuccess, Is.True);
        _schools.Verify(repository => repository.AddAsync(It.IsAny<School>(), It.IsAny<CancellationToken>()), Times.Once);
        var school = _added.Single();
        Assert.That(school.Id, Is.EqualTo(SchoolId));
        Assert.That(school.Name, Is.EqualTo("CEIP Sumi"));
        Assert.That(school.OfficialCode, Is.EqualTo("28000001"));
        Assert.That(school.Address, Is.EqualTo("Calle Mayor 1"));
    }

    [Test]
    public async Task UseCase_WithoutAddress_AddsSchoolWithNullAddress()
    {
        var command = new RegisterSchool.Command(SchoolId, "CEIP Sumi", "28000001", null);

        var result = await _useCase.ExecuteAsync(command);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_added.Single().Address, Is.Null);
    }

    [Test]
    public async Task UseCase_WithEmptyId_ReturnsIdRequiredAndAddsNothing()
    {
        var command = new RegisterSchool.Command(Guid.Empty, "CEIP Sumi", "28000001", null);

        var result = await _useCase.ExecuteAsync(command);

        AssertFailedWith(result, SchoolErrors.IdRequired);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task UseCase_WithBlankName_ReturnsNameRequiredAndAddsNothing(string name)
    {
        var command = new RegisterSchool.Command(SchoolId, name, "28000001", null);

        var result = await _useCase.ExecuteAsync(command);

        AssertFailedWith(result, SchoolErrors.NameRequired);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task UseCase_WithBlankOfficialCode_ReturnsOfficialCodeRequiredAndAddsNothing(string officialCode)
    {
        var command = new RegisterSchool.Command(SchoolId, "CEIP Sumi", officialCode, null);

        var result = await _useCase.ExecuteAsync(command);

        AssertFailedWith(result, SchoolErrors.OfficialCodeRequired);
    }

    [Test]
    public async Task Handler_WithValidCommand_DelegatesToUseCaseAndAddsSchool()
    {
        var command = new RegisterSchool.Command(SchoolId, "CEIP Sumi", "28000001", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_added.Select(school => school.Id), Is.EqualTo(new[] { SchoolId }));
    }

    [Test]
    public async Task Handler_WithInvalidCommand_ReturnsTheUseCaseFailure()
    {
        var command = new RegisterSchool.Command(SchoolId, "", "28000001", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        AssertFailedWith(result, SchoolErrors.NameRequired);
    }

    private void AssertFailedWith(Result result, Error expectedError)
    {
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(expectedError));
        _schools.Verify(repository => repository.AddAsync(It.IsAny<School>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
