using MediatR;
using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// The root command opens a database transaction. Once it succeeds, the domain events raised by the aggregates are
/// pulled and their handlers run in the same transaction; only if all of them succeed is everything saved and
/// committed. An exception (also from an event handler) rolls everything back and propagates.
/// A unique-key violation (<see cref="DuplicateKeyException"/>) also rolls back, but becomes
/// <see cref="SharedExceptions.AlreadyExists"/> (HTTP 409). A lost optimistic-concurrency race
/// (<see cref="ConcurrencyConflictException"/>) behaves the same way and becomes
/// <see cref="SharedExceptions.ConcurrencyConflict"/> (HTTP 409).
/// Use cases never touch the unit of work.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, IDomainEventDispatcher dispatcher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (unitOfWork.HasActiveTransaction)
        {
            var nestedResponse = await next();
            await unitOfWork.SaveChangesAsync(ct);

            return nestedResponse;
        }

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var response = await next();

            await dispatcher.DispatchAsync(ct);

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return response;
        }
        catch (DuplicateKeyException)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw new SharedExceptions.AlreadyExists();
        }
        catch (ConcurrencyConflictException)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw new SharedExceptions.ConcurrencyConflict();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
