using MediatR;
using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Shared.Application;

[TestFixture]
public class UnitOfWorkBehaviorTests
{
    private static readonly Error HandlerError = new("Test.HandlerFailed", "El manejador ha fallado.");
    private static readonly Error DispatchError = new("Test.DispatchFailed", "Un manejador de eventos ha fallado.");

    private sealed record PlainCommand : ICommand, IRequest<Result>;

    private sealed record ValueCommand : ICommand, IRequest<Result<int>>;

    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IDomainEventDispatcher> _dispatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.HasActiveTransaction).Returns(false);
        _dispatcher = new Mock<IDomainEventDispatcher>();
        _dispatcher.Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
    }

    private UnitOfWorkBehavior<PlainCommand, Result> PlainBehavior() => new(_unitOfWork.Object, _dispatcher.Object);

    private UnitOfWorkBehavior<ValueCommand, Result<int>> ValueBehavior() => new(_unitOfWork.Object, _dispatcher.Object);

    private void VerifyRolledBackOnce() => _unitOfWork.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyNeverCommitted() => _unitOfWork.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);

    [Test]
    public async Task Handle_WithSuccessfulHandler_DispatchesEventsSavesAndCommits()
    {
        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(response.IsSuccess, Is.True);
        _unitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcher.Verify(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithFailedHandler_RollsBackAndSkipsDispatchSaveAndCommit()
    {
        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Failure(HandlerError)), CancellationToken.None);

        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(HandlerError));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
        _dispatcher.Verify(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithFailedEventHandler_RollsBackAndReturnsItsErrorWithoutSaving()
    {
        _dispatcher.Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure(DispatchError));

        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(DispatchError));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenSavingViolatesAUniqueKey_RollsBackAndReturnsTheSharedAlreadyExistsError()
    {
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateKeyException("PK_Classes", new InvalidOperationException("inner")));

        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(SharedErrors.AlreadyExists));
        Assert.That(response.Error!.Code, Is.EqualTo("Shared.AlreadyExists"));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
    }

    [Test]
    public async Task Handle_WhenSavingViolatesAUniqueKey_ReturnsAFailedGenericResult()
    {
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateKeyException(null, new InvalidOperationException("inner")));

        var response = await ValueBehavior().Handle(new ValueCommand(), _ => Task.FromResult(Result<int>.Success(7)), CancellationToken.None);

        Assert.That(response, Is.InstanceOf<Result<int>>());
        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(SharedErrors.AlreadyExists));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
    }

    [Test]
    public async Task Handle_WhenTheHandlerRaisesAUniqueKeyViolation_RollsBackAndReturnsTheSharedError()
    {
        var response = await PlainBehavior().Handle(
            new PlainCommand(),
            _ => throw new DuplicateKeyException("IX_Users_Email", new InvalidOperationException("inner")),
            CancellationToken.None);

        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(SharedErrors.AlreadyExists));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
        _dispatcher.Verify(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenDispatchingEventsViolatesAUniqueKey_RollsBackAndReturnsTheSharedError()
    {
        _dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateKeyException("PK_Users", new InvalidOperationException("inner")));

        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(SharedErrors.AlreadyExists));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenSavingLosesAConcurrencyRace_RollsBackAndReturnsTheSharedConcurrencyConflictError()
    {
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException(new InvalidOperationException("inner")));

        var response = await ValueBehavior().Handle(new ValueCommand(), _ => Task.FromResult(Result<int>.Success(7)), CancellationToken.None);

        Assert.That(response, Is.InstanceOf<Result<int>>());
        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.EqualTo(SharedErrors.ConcurrencyConflict));
        Assert.That(response.Error!.Code, Is.EqualTo("Shared.ConcurrencyConflict"));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
    }

    [Test]
    public async Task Handle_WhenTheHandlerRaisesAConcurrencyConflict_RollsBackAndReturnsTheSharedError()
    {
        var response = await PlainBehavior().Handle(
            new PlainCommand(),
            _ => throw new ConcurrencyConflictException(new InvalidOperationException("inner")),
            CancellationToken.None);

        Assert.That(response.Error, Is.EqualTo(SharedErrors.ConcurrencyConflict));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
        _dispatcher.Verify(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenDispatchingEventsLosesAConcurrencyRace_RollsBackAndReturnsTheSharedError()
    {
        _dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException(new InvalidOperationException("inner")));

        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(response.Error, Is.EqualTo(SharedErrors.ConcurrencyConflict));
        VerifyRolledBackOnce();
        VerifyNeverCommitted();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenSavingThrowsAnotherException_RollsBackAndPropagatesIt()
    {
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None));

        VerifyRolledBackOnce();
        VerifyNeverCommitted();
    }

    [Test]
    public async Task Handle_WithinAnActiveTransaction_OnlySavesAndNeverBeginsOrCommits()
    {
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.HasActiveTransaction).Returns(true);

        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(response.IsSuccess, Is.True);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNeverCommitted();
    }

    [Test]
    public async Task Handle_WithinAnActiveTransaction_AndAFailedHandler_DoesNotSave()
    {
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.HasActiveTransaction).Returns(true);

        var response = await PlainBehavior().Handle(new PlainCommand(), _ => Task.FromResult(Result.Failure(HandlerError)), CancellationToken.None);

        Assert.That(response.IsFailure, Is.True);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
