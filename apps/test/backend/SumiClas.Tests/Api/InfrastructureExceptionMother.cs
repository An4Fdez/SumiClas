namespace SumiClas.Tests.Api;

using Npgsql;
using SumiClas.WeeklySummaries.Application;

public static class InfrastructureExceptionMother
{
    public const string INTERNAL_DETAIL = "relation Users does not exist, password=hunter2";

    public static Exception Unexpected() => new InvalidOperationException(INTERNAL_DETAIL);

    public static WeeklySummaryWriterException WriterFailure()
        => new(INTERNAL_DETAIL, new IOException());

    public static NpgsqlException TransientDatabase() => new("The database is down.", new IOException());

    public static NpgsqlException DatabaseTimeout() => new("The command timed out.", new TimeoutException());

    public static NpgsqlException NonTransientDatabase() => new(INTERNAL_DETAIL);

    public static Exception WrappedTransientDatabase()
        => new InvalidOperationException("The retries were exhausted.", TransientDatabase());
}
