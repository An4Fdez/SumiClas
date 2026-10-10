using Microsoft.EntityFrameworkCore;
using Npgsql;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Tests.Shared.Infrastructure;

[TestFixture]
public class UniqueViolationDetectorTests
{
    private static PostgresException Postgres(string sqlState, string? constraintName = null)
        => new("duplicate key value violates unique constraint", "ERROR", "ERROR", sqlState, constraintName: constraintName);

    [Test]
    public void TryGetConstraintName_WithUniqueViolation_ReturnsTrueAndTheConstraint()
    {
        var exception = new DbUpdateException("save failed", Postgres("23505", "PK_Classes"));

        var detected = UniqueViolationDetector.TryGetConstraintName(exception, out var constraintName);

        Assert.That(detected, Is.True);
        Assert.That(constraintName, Is.EqualTo("PK_Classes"));
    }

    [Test]
    public void TryGetConstraintName_WithUniqueViolationWithoutConstraint_ReturnsTrueAndNull()
    {
        var exception = new DbUpdateException("save failed", Postgres("23505"));

        var detected = UniqueViolationDetector.TryGetConstraintName(exception, out var constraintName);

        Assert.That(detected, Is.True);
        Assert.That(constraintName, Is.Null);
    }

    [TestCase("23503")]
    [TestCase("23502")]
    [TestCase("40001")]
    [TestCase("42P01")]
    public void TryGetConstraintName_WithAnotherSqlState_ReturnsFalse(string sqlState)
    {
        var exception = new DbUpdateException("save failed", Postgres(sqlState, "FK_Something"));

        var detected = UniqueViolationDetector.TryGetConstraintName(exception, out var constraintName);

        Assert.That(detected, Is.False);
        Assert.That(constraintName, Is.Null);
    }

    [Test]
    public void TryGetConstraintName_WithConcurrencyException_ReturnsFalseEvenWithAUniqueViolationInside()
    {
        var exception = new DbUpdateConcurrencyException("concurrency", Postgres("23505", "PK_Classes"));

        var detected = UniqueViolationDetector.TryGetConstraintName(exception, out _);

        Assert.That(detected, Is.False);
    }

    [Test]
    public void TryGetConstraintName_WithoutInnerException_ReturnsFalse()
    {
        var exception = new DbUpdateException("save failed");

        var detected = UniqueViolationDetector.TryGetConstraintName(exception, out _);

        Assert.That(detected, Is.False);
    }

    [Test]
    public void TryGetConstraintName_WithANonPostgresInnerException_ReturnsFalse()
    {
        var exception = new DbUpdateException("save failed", new InvalidOperationException("boom"));

        var detected = UniqueViolationDetector.TryGetConstraintName(exception, out _);

        Assert.That(detected, Is.False);
    }
}
