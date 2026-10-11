using System.Reflection;
using SumiClas.Shared.Application;

namespace SumiClas.Api.Configuration;

public static class ConventionServiceCollectionExtensions
{
    /// <summary>
    /// Registers every concrete class named "*Repository" against each interface it implements whose name also ends in "Repository".
    /// </summary>
    public static IServiceCollection AddRepositoriesByConvention(this IServiceCollection services, Assembly assembly)
    {
        var repositories = assembly.GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            && type.Name.EndsWith("Repository", StringComparison.Ordinal));

        foreach (var repository in repositories)
        {
            var contracts = repository.GetInterfaces()
                .Where(contract => contract.Name.EndsWith("Repository", StringComparison.Ordinal));

            foreach (var contract in contracts)
            {
                services.AddScoped(contract, repository);
            }
        }

        return services;
    }

    /// <summary>
    /// Registers every concrete class declared in a "*.Domain.Services" namespace (domain services, e.g. SchoolFinder).
    /// </summary>
    public static IServiceCollection AddDomainServicesByConvention(this IServiceCollection services, Assembly assembly)
    {
        var domainServices = assembly.GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            && type.Namespace is not null
            && type.Namespace.EndsWith(".Domain.Services", StringComparison.Ordinal));

        foreach (var domainService in domainServices)
        {
            services.AddScoped(domainService);
        }

        return services;
    }

    /// <summary>
    /// Registers every concrete class that implements IDomainEventHandler&lt;TEvent&gt; against that interface
    /// (e.g. UserClassFirstRepresentativeAssignOnClassCreate); a class may handle several events.
    /// </summary>
    public static IServiceCollection AddDomainEventHandlersByConvention(this IServiceCollection services, Assembly assembly)
    {
        var handlers = assembly.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var handler in handlers)
        {
            var contracts = handler.GetInterfaces().Where(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var contract in contracts)
            {
                services.AddScoped(contract, handler);
            }
        }

        return services;
    }

    /// <summary>
    /// Registers every concrete public class that implements IUseCase (nested in a static class, e.g. SchoolYearCreate.Create), whatever its name.
    /// </summary>
    public static IServiceCollection AddUseCasesByConvention(this IServiceCollection services, Assembly assembly)
    {
        var useCases = assembly.GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            && typeof(IUseCase).IsAssignableFrom(type));

        foreach (var useCase in useCases)
        {
            services.AddScoped(useCase);
        }

        return services;
    }
}
