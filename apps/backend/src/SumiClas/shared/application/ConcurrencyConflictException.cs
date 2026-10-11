namespace SumiClas.Shared.Application;

/// <summary>
/// A concurrency token no longer matched while saving: another request changed the same row first. It is raised by the
/// infrastructure so the application layer does not depend on EF Core; <see cref="UnitOfWorkBehavior{TRequest,TResponse}"/> turns it into a failed result.
/// </summary>
public sealed class ConcurrencyConflictException(Exception innerException)
    : Exception("A concurrency conflict happened while saving.", innerException);
