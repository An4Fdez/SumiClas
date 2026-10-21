namespace SumiClas.Tests.Shared.Domain;

using AwesomeAssertions;
using SumiClas.Shared.Domain;

[TestFixture]
public class ResultOrThrowShould
{
    [Test]
    public void return_the_value_of_a_successful_result()
    {
        int value = System.Random.Shared.Next();

        int actual = Result<int>.Success(value).OrThrow();

        actual.Should().Be(value);
    }

    [Test]
    public void do_nothing_for_a_successful_result_without_value()
    {
        Action act = () => Result.Success().OrThrow();

        act.Should().NotThrow();
    }

    [Test]
    public void throw_the_error_of_a_failed_result_without_value()
    {
        Error error = ErrorMother.Random();

        Action act = () => Result.Failure(error).OrThrow();

        ResultFailureException thrown = act.Should().Throw<ResultFailureException>().Which;
        thrown.Error.Should().Be(error);
        thrown.Code.Should().Be(error.Code);
        thrown.Message.Should().Be(error.Message);
    }

    [Test]
    public void throw_the_error_of_a_failed_result_with_value()
    {
        Error error = ErrorMother.Random();

        Action act = () => Result<int>.Failure(error).OrThrow();

        act.Should().Throw<ResultFailureException>().Which.Error.Should().Be(error);
    }
}
