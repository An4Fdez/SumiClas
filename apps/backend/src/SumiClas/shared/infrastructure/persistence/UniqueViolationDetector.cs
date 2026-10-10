using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SumiClas.Shared.Infrastructure.Persistence;

public static class UniqueViolationDetector
{
    /// <summary>
    /// True only for a PostgreSQL unique_violation (23505) wrapped in a <see cref="DbUpdateException"/>.
    /// Concurrency exceptions and every other SqlState are not unique violations and must stay unhandled.
    /// </summary>
    public static bool TryGetConstraintName(DbUpdateException exception, out string? constraintName)
    {
        constraintName = null;
        if (exception is DbUpdateConcurrencyException)
        {
            return false;
        }

        if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            constraintName = postgres.ConstraintName;
            return true;
        }

        return false;
    }
}
