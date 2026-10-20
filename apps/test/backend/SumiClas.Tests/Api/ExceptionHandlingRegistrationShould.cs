namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;

[TestFixture]
public class ExceptionHandlingRegistrationShould
{
    [Test]
    public void register_the_domain_exception_handler_and_the_problem_details_service()
    {
        using ServiceProvider services = new ServiceCollection()
            .AddLogging()
            .AddSumiClasExceptionHandling()
            .BuildServiceProvider();

        services.GetServices<IExceptionHandler>()
            .Should().ContainSingle().Which.Should().BeOfType<DomainExceptionHandler>();
        services.GetService<IProblemDetailsService>().Should().NotBeNull();
    }
}
