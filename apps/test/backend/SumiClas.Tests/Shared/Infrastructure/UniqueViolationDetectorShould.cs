namespace SumiClas.Tests.Shared.Infrastructure;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Infrastructure.Persistence;

[TestFixture]
public class UniqueViolationDetectorShould
{
    [Test]
    public void return_true_and_the_constraint_when_it_is_a_unique_violation()
    {
        string constraint = StringMother.Random("constraint");
        DbUpdateException exception = DbUpdateExceptionMother.With(
            PostgresExceptionMother.With(constraintName: constraint));

        bool detected = UniqueViolationDetector.TryGetConstraintName(exception, out string? constraintName);

        detected.Should().BeTrue();
        constraintName.Should().Be(constraint);
    }

    [Test]
    public void return_true_and_null_when_it_is_a_unique_violation_without_constraint()
    {
        DbUpdateException exception = DbUpdateExceptionMother.With(PostgresExceptionMother.WithoutConstraint());

        bool detected = UniqueViolationDetector.TryGetConstraintName(exception, out string? constraintName);

        detected.Should().BeTrue();
        constraintName.Should().BeNull();
    }

    [TestCase("23503")]
    [TestCase("23502")]
    [TestCase("40001")]
    [TestCase("42P01")]
    public void return_false_when_the_sql_state_is_another_one(string sqlState)
    {
        DbUpdateException exception = DbUpdateExceptionMother.With(
            PostgresExceptionMother.With(sqlState, StringMother.Random("constraint")));

        bool detected = UniqueViolationDetector.TryGetConstraintName(exception, out string? constraintName);

        detected.Should().BeFalse();
        constraintName.Should().BeNull();
    }

    [Test]
    public void return_false_when_it_is_a_concurrency_exception_even_with_a_unique_violation_inside()
    {
        DbUpdateException exception = DbUpdateExceptionMother.Concurrency(
            PostgresExceptionMother.With(constraintName: StringMother.Random("constraint")));

        bool detected = UniqueViolationDetector.TryGetConstraintName(exception, out _);

        detected.Should().BeFalse();
    }

    [Test]
    public void return_false_when_there_is_no_inner_exception()
    {
        DbUpdateException exception = DbUpdateExceptionMother.WithoutInner();

        bool detected = UniqueViolationDetector.TryGetConstraintName(exception, out _);

        detected.Should().BeFalse();
    }

    [Test]
    public void return_false_when_the_inner_exception_is_not_a_postgres_one()
    {
        DbUpdateException exception = DbUpdateExceptionMother.With(
            new InvalidOperationException(StringMother.Random("error")));

        bool detected = UniqueViolationDetector.TryGetConstraintName(exception, out _);

        detected.Should().BeFalse();
    }
}
