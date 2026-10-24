namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Infrastructure;

[TestFixture]
public class AiOptionsShould
{
    private const int ONE = 1;

    [Test]
    public void be_off_and_valid_when_there_is_no_key()
    {
        AiOptions options = Resolve(new Dictionary<string, string?>());

        options.IsEnabled.Should().BeFalse();
        options.ApiKey.Should().BeEmpty();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void stay_off_when_the_key_is_blank(string key)
    {
        AiOptions options = Resolve(Settings(("ApiKey", key)));

        options.IsEnabled.Should().BeFalse();
    }

    [Test]
    public void be_on_when_there_is_a_key()
    {
        string key = StringMother.Random("key");

        AiOptions options = Resolve(Settings(("ApiKey", key)));

        options.IsEnabled.Should().BeTrue();
        options.ApiKey.Should().Be(key);
    }

    [Test]
    public void use_the_defaults_when_only_the_key_is_given()
    {
        AiOptions options = Resolve(Settings(("ApiKey", StringMother.Random("key"))));

        options.Model.Should().Be(AiOptions.DefaultModel);
        options.TimeoutSeconds.Should().Be(AiOptions.DefaultTimeoutSeconds);
        options.MaxRetries.Should().Be(AiOptions.DefaultMaxRetries);
    }

    [Test]
    public void ship_in_appsettings_the_same_values_as_its_defaults()
    {
        IConfiguration shipped = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
            .Build();
        ServiceCollection services = new();
        services.AddSumiClasAiOptions(shipped);
        using ServiceProvider provider = services.BuildServiceProvider();

        AiOptions options = provider.GetRequiredService<IOptions<AiOptions>>().Value;

        options.Should().BeEquivalentTo(new AiOptions());
    }

    [Test]
    public void read_the_model_the_timeout_and_the_retries_from_configuration()
    {
        string model = StringMother.Random("model");

        AiOptions options = Resolve(Settings(
            ("Model", model),
            ("TimeoutSeconds", AiOptions.MaxTimeoutSeconds.ToString()),
            ("MaxRetries", AiOptions.MaxRetriesAllowed.ToString())));

        options.Model.Should().Be(model);
        options.TimeoutSeconds.Should().Be(AiOptions.MaxTimeoutSeconds);
        options.MaxRetries.Should().Be(AiOptions.MaxRetriesAllowed);
    }

    [TestCase(ONE)]
    [TestCase(AiOptions.MaxTimeoutSeconds)]
    public void accept_a_timeout_inside_the_range(int seconds)
    {
        AiOptions options = Resolve(Settings(("TimeoutSeconds", seconds.ToString())));

        options.TimeoutSeconds.Should().Be(seconds);
    }

    [TestCase(0)]
    [TestCase(AiOptions.MaxRetriesAllowed)]
    public void accept_a_number_of_retries_inside_the_range(int retries)
    {
        AiOptions options = Resolve(Settings(("MaxRetries", retries.ToString())));

        options.MaxRetries.Should().Be(retries);
    }

    [Test]
    public void fail_with_the_name_of_the_setting_when_the_model_is_blank()
    {
        Func<AiOptions> act = () => Resolve(Settings(("Model", " ")));

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Ai:Model");
    }

    [TestCase(0)]
    [TestCase(-ONE)]
    [TestCase(AiOptions.MaxTimeoutSeconds + ONE)]
    public void fail_with_the_name_of_the_setting_when_the_timeout_is_out_of_range(int seconds)
    {
        Func<AiOptions> act = () => Resolve(Settings(("TimeoutSeconds", seconds.ToString())));

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Ai:TimeoutSeconds");
    }

    [TestCase(-ONE)]
    [TestCase(AiOptions.MaxRetriesAllowed + ONE)]
    public void fail_with_the_name_of_the_setting_when_the_retries_are_out_of_range(int retries)
    {
        Func<AiOptions> act = () => Resolve(Settings(("MaxRetries", retries.ToString())));

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Ai:MaxRetries");
    }

    [Test]
    public void be_checked_when_the_application_starts_even_with_no_key()
    {
        ServiceCollection services = new();
        services.AddSumiClasAiOptions(ConfigurationMother.Create(Settings(("TimeoutSeconds", "0"))));
        using ServiceProvider provider = services.BuildServiceProvider();

        IStartupValidator validator = provider.GetRequiredService<IStartupValidator>();

        Action validate = validator.Validate;

        validate.Should().ThrowExactly<OptionsValidationException>();
    }

    private static Dictionary<string, string?> Settings(params (string Name, string Value)[] settings)
        => settings.ToDictionary(
            setting => $"{AiOptions.SectionName}:{setting.Name}",
            setting => (string?)setting.Value);

    private static AiOptions Resolve(IReadOnlyDictionary<string, string?> settings)
    {
        ServiceCollection services = new();
        services.AddSumiClasAiOptions(ConfigurationMother.Create(settings));
        using ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<AiOptions>>().Value;
    }
}
