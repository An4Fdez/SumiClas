using System.Reflection;

namespace SumiClas.Api.Bootstrap;

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
    /// Registers every public nested class named "UseCase" declared inside a static class (e.g. RegisterSchool.UseCase).
    /// </summary>
    public static IServiceCollection AddUseCasesByConvention(this IServiceCollection services, Assembly assembly)
    {
        var useCases = assembly.GetTypes().Where(type =>
            type is { IsNestedPublic: true, IsClass: true, Name: "UseCase" }
            && type.DeclaringType is { IsAbstract: true, IsSealed: true });

        foreach (var useCase in useCases)
        {
            services.AddScoped(useCase);
        }

        return services;
    }
}
