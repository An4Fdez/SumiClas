using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

public interface IDomainEventDispatcher
{
    /// <summary>
    /// Pulls the pending domain events from the aggregates tracked in the current unit of work and runs their
    /// handlers, repeating while handlers raise new events. A handler that throws stops the dispatch.
    /// </summary>
    Task DispatchAsync(CancellationToken ct = default);
}
