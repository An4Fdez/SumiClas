using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Shared.Application;

namespace SumiClas.Tests.Api;

[TestFixture]
public class ApiSecurityRegistrationTests
{
    [Test]
    public void AddSumiClasApiSecurity_RegistersTheJwtUserProvider_AndTheBearerOptionsResolve()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "sumiclas-api",
            ["Jwt:Audience"] = "sumiclas-web",
            ["Jwt:SigningKey"] = new string('k', 32),
        }).Build();
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddSumiClasJwtOptions(configuration)
            .AddSumiClasApiSecurity(configuration)
            .BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        var bearer = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.That(scope.ServiceProvider.GetRequiredService<IUserProvider>(), Is.TypeOf<JwtUserProvider>());
        Assert.That(bearer.TokenValidationParameters.ValidIssuer, Is.EqualTo("sumiclas-api"));
    }
}
