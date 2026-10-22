namespace SumiClas.Tests.Shared.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Shared.Application;
using SumiClas.Tests.Api;
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
    public async Task roll_back_and_propagate_it_without_saving_when_an_event_handler_throws()
    {
        this.ShouldThrowDispatching(new SampleExceptions.NotFound());

        Func<Task> act = () => this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        await act.Should().ThrowAsync<SampleExceptions.NotFound>();
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_return_the_shared_already_exists_error_when_saving_violates_a_unique_key()
    {
        this.ShouldDispatchEvents();
        this.ShouldThrowSaving(DuplicateKeyExceptionMother.With(StringMother.Random("constraint")));

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        response.IsFailure.Should().BeTrue();
        response.Error.Should().Be(SharedErrors.AlreadyExists);
        response.Error!.Code.Should().Be("Shared.AlreadyExists");
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
        this.ShouldThrowDispatching(DuplicateKeyExceptionMother.With(StringMother.Random("constraint")));

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
        this.ShouldThrowSaving(ConcurrencyConflictExceptionMother.Random());

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
    public async Task roll_back_and_propagate_it_when_saving_throws_another_exception()
    {
        this.ShouldDispatchEvents();
        this.ShouldThrowSaving(new InvalidOperationException(StringMother.Random("error")));

        Func<Task> act = () => this.HandlePlainAsync(() => Task.FromResult(Result.Success()));

        await act.Should().ThrowAsync<InvalidOperationException>();
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task only_save_and_never_begin_or_commit_when_there_is_an_active_transaction()
    {
        this.ShouldHaveActiveTransaction();

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
        this.ShouldHaveActiveTransaction();

        Result response = await this.HandlePlainAsync(() => Task.FromResult(Result.Failure(handlerError)));

        response.IsFailure.Should().BeTrue();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task dispatch_events_save_and_commit_when_a_command_that_returns_a_value_succeeds()
    {
        this.ShouldDispatchEvents();
        int value = System.Random.Shared.Next();

        int response = await this.HandleConvertedAsync(() => Task.FromResult(value));

        response.Should().Be(value);
        this.VerifyBeganOnce();
        this.VerifyDispatchedOnce();
        this.VerifySavedOnce();
        this.VerifyCommittedOnce();
        this.VerifyNeverRolledBack();
    }

    [Test]
    public async Task roll_back_and_propagate_the_domain_exception_when_a_command_that_returns_a_value_throws_it()
    {
        Func<Task> act = () => this.HandleConvertedAsync(() => throw new SampleExceptions.NotFound());

        await act.Should().ThrowAsync<SampleExceptions.NotFound>();
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverDispatched();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_propagate_it_when_an_event_handler_throws_and_the_command_returns_a_value()
    {
        this.ShouldThrowDispatching(new SampleExceptions.NotFound());

        Func<Task> act = () => this.HandleConvertedAsync(() => Task.FromResult(System.Random.Shared.Next()));

        await act.Should().ThrowAsync<SampleExceptions.NotFound>();
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
        this.VerifyNeverSaved();
    }

    [Test]
    public async Task roll_back_and_throw_shared_already_exists_when_saving_violates_a_unique_key_without_a_result()
    {
        this.ShouldDispatchEvents();
        this.ShouldThrowSaving(DuplicateKeyExceptionMother.With(StringMother.Random("constraint")));

        Func<Task> act = () => this.HandleConvertedAsync(() => Task.FromResult(System.Random.Shared.Next()));

        (await act.Should().ThrowAsync<SharedExceptions.AlreadyExists>())
            .Which.Code.Should().Be("Shared.AlreadyExists");
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task roll_back_and_throw_shared_concurrency_conflict_when_saving_loses_a_race_without_a_result()
    {
        this.ShouldDispatchEvents();
        this.ShouldThrowSaving(ConcurrencyConflictExceptionMother.Random());

        Func<Task> act = () => this.HandleConvertedAsync(() => Task.FromResult(System.Random.Shared.Next()));

        (await act.Should().ThrowAsync<SharedExceptions.ConcurrencyConflict>())
            .Which.Code.Should().Be("Shared.ConcurrencyConflict");
        this.VerifyRolledBackOnce();
        this.VerifyNeverCommitted();
    }

    [Test]
    public async Task only_save_when_there_is_an_active_transaction_and_a_command_that_returns_a_value_succeeds()
    {
        this.ShouldHaveActiveTransaction();
        int value = System.Random.Shared.Next();

        int response = await this.HandleConvertedAsync(() => Task.FromResult(value));

        response.Should().Be(value);
        this.VerifySavedOnce();
        this.VerifyNeverBegan();
        this.VerifyNeverCommitted();
    }

    private async Task<int> HandleConvertedAsync(Func<Task<int>> handler)
    {
        UnitOfWorkBehavior<UnitOfWorkCommandMother.ConvertedCommand, int> behavior = new(
            this._unitOfWork.Object,
            this._dispatcher.Object);

        return await behavior.Handle(UnitOfWorkCommandMother.Converted(), _ => handler(), CancellationToken.None);
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
            .Returns(Task.CompletedTask);

    private void ShouldThrowDispatching(Exception exception)
        => this._dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void ShouldThrowSaving(Exception exception)
        => this._unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void ShouldHaveActiveTransaction()
        => this._unitOfWork.SetupGet(unitOfWork => unitOfWork.HasActiveTransaction).Returns(true);

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
