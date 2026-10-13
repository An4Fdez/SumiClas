namespace SumiClas.Tests.Shared.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared.Domain;

[TestFixture]
public class UnitOfWorkBehaviorShould
{

    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDomainEventDispatcher> _dispatcher = new();

    [SetUp]
    public void SetUp()
    {
        this._unitOfWork.Reset();
        this._dispatcher.Reset();
    }

    [Test]
    public async Task dispatch_events_save_and_commit_when_the_handler_succeeds()
    {
        this.ShouldDispatchEvents();
        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.IsSuccess.Should().BeTrue();
        this.VerifyBeganOnce();
        this.VerifyDispatchedOnce();
        this.VerifySavedOnce();
        this.VerifyCommittedOnce();
        this.VerifyNeverRolledBack();
    }

    [Test]
    public async Task roll_back_and_skip_dispatch_save_and_commit_when_the_handler_fails()
    {
        Error handlerError = ErrorMother.Random();

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Failure(handlerError)));

        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(handlerError);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverDispatched();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_return_its_error_without_saving_when_an_event_handler_fails()
    {
        Error dispatchError = ErrorMother.Random();
        this._dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(dispatchError));

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(dispatchError);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_already_exists_error_when_saving_violates_a_unique_key()
    {
        this.ShouldDispatchEvents();
        this._unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(DuplicateKeyExceptionMother.With(StringMother.Random("constraint")));

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(SharedErrors.AlreadyExists);
        response.Error!.Code.Should().Be("Shared.AlreadyExists");
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task return_a_failed_generic_result_when_saving_violates_a_unique_key()
    {
        this.ShouldDispatchEvents();
        this._unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(DuplicateKeyExceptionMother.WithoutConstraint());

        int value = System.Random.Shared.Next();

        Result<int> response = await this.HandleValueAsync(() => Task.FromResult(Result<int>.Success(value)));

        response.Should().BeOfType<Result<int>>();
        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(SharedErrors.AlreadyExists);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_error_when_the_handler_raises_a_unique_key_violation()
    {
        Result response = await this.HandlePlainAsync(
            () => throw DuplicateKeyExceptionMother.With(StringMother.Random("constraint")));

        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(SharedErrors.AlreadyExists);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverDispatched();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_error_when_dispatching_events_violates_a_unique_key()
    {
        this._dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(DuplicateKeyExceptionMother.With(StringMother.Random("constraint")));

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(SharedErrors.AlreadyExists);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_concurrency_conflict_error_when_saving_loses_a_race()
    {
        this.ShouldDispatchEvents();
        this._unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(ConcurrencyConflictExceptionMother.Random());

        int value = System.Random.Shared.Next();

        Result<int> response = await this.HandleValueAsync(() => Task.FromResult(Result<int>.Success(value)));

        response.Should().BeOfType<Result<int>>();
        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(SharedErrors.ConcurrencyConflict);
        response.Error!.Code.Should().Be("Shared.ConcurrencyConflict");
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_error_when_the_handler_raises_a_concurrency_conflict()
    {
        Result response = await this.HandlePlainAsync(() => throw ConcurrencyConflictExceptionMother.Random());

        response.Error.Should().Be(SharedErrors.ConcurrencyConflict);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverDispatched();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_error_when_dispatching_events_loses_a_race()
    {
        this._dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(ConcurrencyConflictExceptionMother.Random());

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.Error.Should().Be(SharedErrors.ConcurrencyConflict);
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_propagate_it_when_saving_throws_another_exception()
    {
        this.ShouldDispatchEvents();
        this._unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(StringMother.Random("error")));

        Func<Task> act = () => this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        await act.Should().ThrowAsync<InvalidOperationException>();
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task only_save_and_never_begin_or_commit_when_there_is_an_active_transaction()
    {
        this._unitOfWork.SetupGet(unitOfWork => unitOfWork.HasActiveTransaction).Returns(true);

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.IsSuccess.Should().BeTrue();
        this.VerifySavedOnce();
        this.VerifyNeverBegan();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task not_save_when_there_is_an_active_transaction_and_the_handler_fails()
    {
        Error handlerError = ErrorMother.Random();
        this._unitOfWork.SetupGet(unitOfWork => unitOfWork.HasActiveTransaction).Returns(true);

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Failure(handlerError)));

        response.IsFailure.Should().BeTrue();
        this.VerifyNeverSaved();
    }

    private async Task<Result> HandlePlainAsync(Func<Task<Result>> handler)
    {
        UnitOfWorkBehavior<UnitOfWorkCommandMother.PlainCommand, Result> behavior = new(
            this._unitOfWork.Object,
            this._dispatcher.Object);

        return await behavior.Handle(UnitOfWorkCommandMother.Plain(), _ => handler(), CancellationToken.None);
    }

    private async Task<Result<int>> HandleValueAsync(Func<Task<Result<int>>> handler)
    {
        UnitOfWorkBehavior<UnitOfWorkCommandMother.ValueCommand, Result<int>> behavior = new(
            this._unitOfWork.Object,
            this._dispatcher.Object);

        return await behavior.Handle(UnitOfWorkCommandMother.Value(), _ => handler(), CancellationToken.None);
    }

    private void ShouldDispatchEvents()
        => this._dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    private void VerifyBeganOnce()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyNeverBegan()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

    private void VerifyDispatchedOnce()
        => this._dispatcher.Verify(
            dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyNeverDispatched()
        => this._dispatcher.Verify(
            dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()),
            Times.Never);

    private void VerifySavedOnce()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyNeverSaved()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

    private void VerifyCommittedOnce()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyNeverCommitted()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

    private void VerifyRolledBackOnce()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyNeverRolledBack()
        => this._unitOfWork.Verify(
            unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
}
