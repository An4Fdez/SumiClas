using System.Reflection;
using MediatR;
using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// The root command opens a database transaction. Once it succeeds, the domain events raised by the aggregates are
/// pulled and their handlers run in the same transaction; only if all of them succeed is everything saved and
/// committed. A failed <see cref="Result"/>, a failed handler or an exception rolls everything back.
/// A unique-key violation (<see cref="DuplicateKeyException"/>) also rolls back, but becomes the failed result
/// <see cref="SharedErrors.AlreadyExists"/> (HTTP 409) instead of an exception. A lost optimistic-concurrency race
/// (<see cref="ConcurrencyConflictException"/>) behaves the same way and becomes
/// <see cref="SharedErrors.ConcurrencyConflict"/> (HTTP 409).
/// Temporary bridge: a command that already returns its value instead of a <see cref="Result"/> fails by throwing, so
/// those two conditions become <see cref="SharedExceptions"/> and a failed event handler a
/// <see cref="ResultFailureException"/>.
/// Use cases never touch the unit of work.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, IDomainEventDispatcher dispatcher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>
{
    private static readonly bool ResultResponse = typeof(Result).IsAssignableFrom(typeof(TResponse));

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (unitOfWork.HasActiveTransaction)
        {
            var nestedResponse = await next();
            if (!IsFailure(nestedResponse))
            {
                await unitOfWork.SaveChangesAsync(ct);
            }

            return nestedResponse;
        }

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var response = await next();
            if (IsFailure(response))
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return response;
            }

            var dispatch = await dispatcher.DispatchAsync(ct);
            if (dispatch.IsFailure)
            {
                if (!ResultResponse)
                {
                    throw new ResultFailureException(dispatch.Error!);
                }

                await unitOfWork.RollbackTransactionAsync(ct);
                return FailedResponse(dispatch.Error!);
            }

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return response;
        }
        catch (DuplicateKeyException)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return Fail(SharedErrors.AlreadyExists, new SharedExceptions.AlreadyExists());
        }
        catch (ConcurrencyConflictException)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return Fail(SharedErrors.ConcurrencyConflict, new SharedExceptions.ConcurrencyConflict());
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    private static bool IsFailure(TResponse response) => response is Result { IsFailure: true };

    private static TResponse Fail(Error error, DomainException exception)
        => ResultResponse ? FailedResponse(error) : throw exception;

    private static TResponse FailedResponse(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var failure = typeof(TResponse).GetMethod(
            nameof(Result.Failure),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
            [typeof(Error)])!;
        return (TResponse)failure.Invoke(null, [error])!;
    }
}
