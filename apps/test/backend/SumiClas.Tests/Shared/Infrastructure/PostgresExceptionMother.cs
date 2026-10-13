namespace SumiClas.Tests.Shared.Infrastructure;

using Npgsql;

public static class PostgresExceptionMother
{
    public const string UNIQUE_VIOLATION = "23505";

    public static PostgresException Create(string sqlState, string? constraintName)
        => new(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            sqlState,
            constraintName: constraintName);

    public static PostgresException With(string? sqlState = null, string? constraintName = null)
        => Create(sqlState ?? UNIQUE_VIOLATION, constraintName ?? StringMother.Random("constraint"));

    public static PostgresException WithoutConstraint(string sqlState = UNIQUE_VIOLATION)
        => Create(sqlState, null);
}
