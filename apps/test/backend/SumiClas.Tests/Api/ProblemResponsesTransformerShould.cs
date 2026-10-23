namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SumiClas.Api.Http;
using System.Text.Json;

/// <summary>The real OpenAPI generator with the transformer, over a loopback socket, reading what it serves.</summary>
[TestFixture]
public class ProblemResponsesTransformerShould
{
    private const string PROBLEM_CONTENT_TYPE = "application/problem+json";
    private const string PROBLEM_SCHEMA_REFERENCE = "#/components/schemas/ProblemDetails";

    [Test]
    public async Task document_the_read_errors_of_a_protected_operation_as_problem_details()
    {
        JsonElement responses = await ResponsesOf("get", "/protected");

        StatusesOf(responses).Should().BeEquivalentTo(["400", "401", "403", "404"]);
        foreach (JsonProperty response in responses.EnumerateObject().Where(response => response.Name != "200"))
        {
            SchemaOf(response.Value).Should().Contain(PROBLEM_SCHEMA_REFERENCE);
        }
    }

    [Test]
    public async Task document_the_conflict_too_when_a_protected_operation_writes()
    {
        JsonElement responses = await ResponsesOf("post", "/protected");

        StatusesOf(responses).Should().BeEquivalentTo(["400", "401", "403", "404", "409"]);
    }

    [Test]
    public async Task document_only_the_authentication_errors_of_an_anonymous_read()
    {
        JsonElement responses = await ResponsesOf("get", "/anonymous");

        StatusesOf(responses).Should().BeEquivalentTo(["400", "401"]);
    }

    [Test]
    public async Task document_the_conflict_but_not_the_missing_or_forbidden_of_an_anonymous_write()
    {
        JsonElement responses = await ResponsesOf("post", "/anonymous");

        StatusesOf(responses).Should().BeEquivalentTo(["400", "401", "409"]);
    }

    [Test]
    public async Task leave_alone_a_response_the_endpoint_already_declared_with_its_own_body()
    {
        JsonElement responses = await ResponsesOf("get", "/validated");

        SchemaOf(responses.GetProperty("400")).Should().Contain("HttpValidationProblemDetails");
        SchemaOf(responses.GetProperty("404")).Should().NotContain("HttpValidationProblemDetails");
    }

    [Test]
    public async Task keep_the_description_of_a_bodyless_declared_response_and_give_it_the_problem_body()
    {
        JsonElement responses = await ResponsesOf("get", "/described");

        JsonElement unauthorized = responses.GetProperty("401");
        unauthorized.GetProperty("description").GetString().Should().Be("Unauthorized");
        SchemaOf(unauthorized).Should().Contain(PROBLEM_SCHEMA_REFERENCE);
    }

    [Test]
    public async Task point_every_error_to_one_shared_problem_details_schema_that_the_document_defines()
    {
        JsonElement document = await DocumentOf();

        JsonElement problem = document.GetProperty("components").GetProperty("schemas").GetProperty("ProblemDetails");
        problem.GetProperty("properties").TryGetProperty("detail", out _).Should().BeTrue();
        problem.GetProperty("properties").TryGetProperty("title", out _).Should().BeTrue();
    }

    private static IEnumerable<string> StatusesOf(JsonElement responses)
        => responses.EnumerateObject().Select(response => response.Name).Where(status => status != "200");

    private static string SchemaOf(JsonElement response)
        => response.GetProperty("content").GetProperty(PROBLEM_CONTENT_TYPE).GetProperty("schema").ToString();

    private static async Task<JsonElement> ResponsesOf(string method, string path)
    {
        JsonElement document = await DocumentOf();
        return document.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses");
    }

    private static async Task<JsonElement> DocumentOf()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        OpenApiRegistration.AddOpenApi(builder.Services, options =>
        {
            options.AddDocumentTransformer<ProblemResponsesTransformer>();
            options.AddOperationTransformer<ProblemResponsesTransformer>();
        });
        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapGet("/protected", () => "ok");
        app.MapPost("/protected", () => "ok");
        app.MapGet("/anonymous", () => "ok").AllowAnonymous();
        app.MapPost("/anonymous", () => "ok").AllowAnonymous();
        app.MapGet("/described", () => "ok").Produces(StatusCodes.Status401Unauthorized);
        app.MapGet("/validated", () => "ok").ProducesValidationProblem();
        await app.StartAsync();

        using HttpClient client = new() { BaseAddress = new Uri(app.Urls.First()) };
        using JsonDocument document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        return document.RootElement.Clone();
    }
}
