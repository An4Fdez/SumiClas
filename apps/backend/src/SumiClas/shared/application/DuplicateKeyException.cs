namespace SumiClas.Shared.Application;

/// <summary>
/// A primary key or a unique index was violated while saving. It is raised by the infrastructure so the
/// application layer does not depend on EF Core or Npgsql; <see cref="UnitOfWorkBehavior{TRequest,TResponse}"/> turns it into a failed result.
/// </summary>
public sealed class DuplicateKeyException(string? constraintName, Exception innerException)
    : Exception("A unique key was violated while saving.", innerException)
{
    /// <summary>For logging only: it must never reach the user message.</summary>
    public string? ConstraintName { get; } = constraintName;
}
