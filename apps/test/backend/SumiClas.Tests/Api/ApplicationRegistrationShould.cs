namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SumiClas.Api.Configuration;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class ApplicationRegistrationShould
{
    [Test]
    public async Task resolve_a_query_through_the_mediator_to_its_registered_handler_and_use_case()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddSumiClasApplication()
            .BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Func<Task> act = () => sender.Send(UserSchoolPublishEnsureQueryMother.With(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<UserExceptions.SchoolPublishForbidden>();
    }
}
