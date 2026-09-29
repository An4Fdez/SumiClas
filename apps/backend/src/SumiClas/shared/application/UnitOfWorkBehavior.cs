using MediatR;
using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// The root command opens a database transaction and commits it only if its <see cref="Result"/> is successful;
/// nested commands join it and just flush their changes, so later reads in the same request see them.
/// A failed result or an exception rolls everything back. Use cases never touch the unit of work.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
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
}
