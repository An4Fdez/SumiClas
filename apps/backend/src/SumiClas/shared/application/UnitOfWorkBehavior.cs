using System.Reflection;
using MediatR;
using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// The root command opens a database transaction. Once its <see cref="Result"/> is successful, the domain events
/// raised by the aggregates are pulled and their handlers run in the same transaction; only if all of them succeed
/// is everything saved and committed. A failed result, a failed handler or an exception rolls everything back.
/// Use cases never touch the unit of work.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, IDomainEventDispatcher dispatcher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (unitOfWork.HasActiveTransaction)
        {
            var nestedResponse = await next();
            if (nestedResponse.IsSuccess)
            {
                await unitOfWork.SaveChangesAsync(ct);
            }

            return nestedResponse;
        }

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var response = await next();
            if (response.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return response;
            }

            var dispatch = await dispatcher.DispatchAsync(ct);
            if (dispatch.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return FailedResponse(dispatch.Error!);
            }

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

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
