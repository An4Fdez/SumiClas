using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

public interface IDomainEventDispatcher
{
    /// <summary>
    /// Pulls the pending domain events from the aggregates tracked in the current unit of work and runs their
    /// handlers, repeating while handlers raise new events. Stops at the first failed handler.
    /// </summary>
    Task<Result> DispatchAsync(CancellationToken ct = default);
}
