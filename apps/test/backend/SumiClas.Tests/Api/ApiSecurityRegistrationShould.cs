namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Shared.Application;
using SumiClas.Tests.Sessions.Infrastructure;
using System.Text.Json;

[TestFixture]
public class ApiSecurityRegistrationShould
{
    [Test]
    public void register_the_jwt_user_provider_and_resolve_the_bearer_options()
    {
        JwtOptions options = JwtOptionsMother.Random();
        IConfiguration configuration = ConfigurationMother.Create(JwtSettingsMother.From(options));
        using ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddSumiClasJwtOptions(configuration)
            .AddSumiClasApiSecurity(configuration)
            .BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        JwtBearerOptions bearer = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        scope.ServiceProvider.GetRequiredService<IUserProvider>().Should().BeOfType<JwtUserProvider>();
        bearer.TokenValidationParameters.ValidIssuer.Should().Be(options.Issuer);
    }

    [Test]
    public async Task document_the_bearer_scheme_and_the_standard_errors_in_the_openapi_of_the_real_registration()
    {
        IConfiguration configuration = ConfigurationMother.Create(JwtSettingsMother.From(JwtOptionsMother.Random()));
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSumiClasJwtOptions(configuration).AddSumiClasApiSecurity(configuration);
        OpenApiRegistration.AddOpenApi(builder.Services, _ => { });
        await using WebApplication app = builder.Build();
        app.MapOpenApi().AllowAnonymous();
        app.MapPost("/protected", () => "ok");
        await app.StartAsync();

        using HttpClient client = new() { BaseAddress = new Uri(app.Urls.First()) };
        using JsonDocument document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        JsonElement root = document.RootElement;

        root.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _).Should().BeTrue();
        root.GetProperty("components").GetProperty("schemas").TryGetProperty("ProblemDetails", out _).Should().BeTrue();
        JsonElement responses = root.GetProperty("paths").GetProperty("/protected").GetProperty("post")
            .GetProperty("responses");
        foreach (string status in new[] { "400", "401", "403", "404", "409" })
        {
            responses.GetProperty(status).GetProperty("content").GetProperty("application/problem+json")
                .GetProperty("schema").ToString().Should().Contain("#/components/schemas/ProblemDetails");
        }
    }
}
