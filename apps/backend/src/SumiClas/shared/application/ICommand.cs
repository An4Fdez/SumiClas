namespace SumiClas.Shared.Application;

/// <summary>
/// Marker for write requests. <see cref="UnitOfWorkBehavior{TRequest, TResponse}"/> only wraps requests that implement it,
/// so queries never trigger a save.
/// </summary>
public interface ICommand;
