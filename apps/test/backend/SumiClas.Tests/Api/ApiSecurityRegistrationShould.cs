namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Shared.Application;
using SumiClas.Tests.Sessions.Infrastructure;

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
}
