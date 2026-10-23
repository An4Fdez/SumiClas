namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SumiClas.Api.Configuration;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class ApplicationRegistrationShould
{
    private const int MINIMUM_COMMANDS = 20;

    [Test]
    public async Task resolve_a_query_through_the_mediator_to_its_registered_handler_and_use_case()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddSumiClasApplication()
            .BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Func<Task> act = () => sender.Send(
            UserSchoolPublishEnsureQueryMother.With(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<UserExceptions.SchoolPublishForbidden>();
    }

    /// <summary>
    /// A command that reaches its use case without the unit of work is never saved: the request answers 201 and the
    /// row does not exist. This walks every command of the application, with and without a response.
    /// </summary>
    [Test]
    public void wrap_every_command_in_the_unit_of_work()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();
        List<Type> commands = RequestTypes().Where(IsCommand).ToList();

        List<string> unwrapped = commands
            .Where(command => !IsWrappedInTheUnitOfWork(scope, command))
            .Select(command => command.FullName!)
            .ToList();

        commands.Count.Should().BeGreaterThanOrEqualTo(MINIMUM_COMMANDS);
        unwrapped.Should().BeEmpty();
    }

    [Test]
    public void leave_every_query_outside_the_unit_of_work()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();
        List<Type> queries = RequestTypes().Where(request => !IsCommand(request)).ToList();

        List<string> wrapped = queries
            .Where(query => IsWrappedInTheUnitOfWork(scope, query))
            .Select(query => query.FullName!)
            .ToList();

        queries.Should().NotBeEmpty();
        wrapped.Should().BeEmpty();
    }

    private static ServiceProvider BuildProvider()
        => new ServiceCollection()
            .AddLogging()
            .AddSingleton(new Mock<IUnitOfWork>().Object)
            .AddSingleton(new Mock<IDomainEventDispatcher>().Object)
            .AddSumiClasApplication()
            .BuildServiceProvider();

    private static IEnumerable<Type> RequestTypes()
        => typeof(IUnitOfWork).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IBaseRequest).IsAssignableFrom(type));

    private static bool IsCommand(Type request) => typeof(ICommand).IsAssignableFrom(request);

    private static bool IsWrappedInTheUnitOfWork(IServiceScope scope, Type request)
    {
        Type? typed = request.GetInterfaces()
            .SingleOrDefault(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>));
        Type response = typed is null ? typeof(Unit) : typed.GetGenericArguments()[0];
        Type behavior = typeof(IPipelineBehavior<,>).MakeGenericType(request, response);

        return scope.ServiceProvider.GetServices(behavior)
            .Any(resolved => resolved!.GetType().GetGenericTypeDefinition() == typeof(UnitOfWorkBehavior<,>));
    }
}
