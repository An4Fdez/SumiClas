namespace SumiClas.Tests.WeeklySummaries.Application;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Application;
using SumiClas.WeeklySummaries.Domain;

[TestFixture]
public class WeeklySummaryFingerprintShould
{
    [Test]
    public void be_a_lowercase_sha256_in_hexadecimal_as_long_as_the_summaries_store()
    {
        string fingerprint = WeeklySummaryFingerprint.Of(PromptOf(StringMother.Random("system"), StringMother.Random("user")));

        fingerprint.Should().HaveLength(WeeklySummary.InputHashLength);
        fingerprint.Should().MatchRegex("^[0-9a-f]+$");
    }

    [Test]
    public void change_when_the_data_of_the_week_changes()
    {
        string system = StringMother.Random("system");

        WeeklySummaryFingerprint.Of(PromptOf(system, StringMother.Random("user")))
            .Should().NotBe(WeeklySummaryFingerprint.Of(PromptOf(system, StringMother.Random("user"))));
    }

    [Test]
    public void change_when_the_instructions_to_the_model_change()
    {
        string user = StringMother.Random("user");

        WeeklySummaryFingerprint.Of(PromptOf(StringMother.Random("system"), user))
            .Should().NotBe(WeeklySummaryFingerprint.Of(PromptOf(StringMother.Random("system"), user)));
    }

    [Test]
    public void tell_apart_where_the_instructions_end_and_the_data_start()
    {
        WeeklySummaryFingerprint.Of(PromptOf("ab", "c")).Should().NotBe(WeeklySummaryFingerprint.Of(PromptOf("a", "bc")));
    }

    [Test]
    public void tell_apart_where_the_instructions_end_even_when_the_texts_have_line_breaks()
    {
        WeeklySummaryFingerprint.Of(PromptOf("a\nb", "c"))
            .Should().NotBe(WeeklySummaryFingerprint.Of(PromptOf("a", "b\nc")));
    }

    private static WeeklySummaryPrompt PromptOf(string system, string user) => new(system, user);
}
