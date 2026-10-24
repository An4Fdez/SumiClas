namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using System.Text.Json;

/// <summary>
/// The settings files the API ships with. Secrets live in User Secrets or in environment variables, never in a file
/// that is committed, so no appsettings may carry one under the names the application reads them from.
/// </summary>
[TestFixture]
public class AppSettingsSecretsShould
{
    private static readonly string[] SecretNames = ["ApiKey", "SigningKey"];

    [Test]
    public void ship_the_settings_files_to_be_checked()
    {
        string[] shipped = Directory.GetFiles(AppContext.BaseDirectory, "appsettings*.json")
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToArray();

        shipped.Should().Contain(["appsettings.json", "appsettings.Development.json"]);
    }

    [Test]
    public void carry_no_secret_in_any_appsettings_file()
    {
        List<string> found = [];
        foreach (string file in Directory.GetFiles(AppContext.BaseDirectory, "appsettings*.json"))
        {
            found.AddRange(SecretsIn(File.ReadAllText(file)).Select(path => $"{Path.GetFileName(file)}: {path}"));
        }

        found.Should().BeEmpty();
    }

    [Test]
    public void have_an_ai_section_without_the_key()
    {
        string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
        using JsonDocument document = JsonDocument.Parse(json);

        JsonElement ai = document.RootElement.GetProperty("Ai");

        ai.TryGetProperty("ApiKey", out _).Should().BeFalse();
        ai.TryGetProperty("Model", out _).Should().BeTrue();
    }

    [Test]
    public void spot_a_secret_at_any_depth_so_the_check_above_means_something()
    {
        string key = StringMother.Random("key");
        string json = $$"""
            {
              "Ai": { "ApiKey": "{{key}}" },
              "Jwt": { "Nested": { "SigningKey": "x" } },
              "Other": { "ApiKeys": "y" }
            }
            """;

        IReadOnlyList<string> found = SecretsIn(json);

        found.Should().BeEquivalentTo(["Ai:ApiKey", "Jwt:Nested:SigningKey"]);
    }

    [Test]
    public void spot_a_secret_inside_an_array()
    {
        string json = """{ "Providers": [ { "Name": "a" }, { "Name": "b", "ApiKey": "secret" } ] }""";

        SecretsIn(json).Should().Equal("Providers:ApiKey");
    }

    [Test]
    public void ignore_a_secret_name_with_no_value()
    {
        SecretsIn("""{ "Ai": { "ApiKey": "" } }""").Should().BeEmpty();
    }

    private static IReadOnlyList<string> SecretsIn(string json)
    {
        JsonDocumentOptions options = new() { CommentHandling = JsonCommentHandling.Skip };
        using JsonDocument document = JsonDocument.Parse(json, options);
        List<string> found = [];
        Collect(document.RootElement, string.Empty, found);
        return found;
    }

    private static void Collect(JsonElement element, string path, List<string> found)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                Collect(item, path, found);
            }

            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            string current = path.Length == 0 ? property.Name : $"{path}:{property.Name}";
            bool isSecret = SecretNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase);
            bool hasValue = property.Value.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.Value.GetString());
            if (isSecret && hasValue)
            {
                found.Add(current);
            }

            Collect(property.Value, current, found);
        }
    }
}
