namespace SumiClas.Shared.Application;

/// <summary>
/// The only way a module may read data owned by another module: it sends the owner's query message and never touches its Domain or Infrastructure.
/// </summary>
public interface IQueryBus
{
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
}
