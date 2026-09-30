using SumiClas.Shared.Application;

namespace SumiClas.Api.Bootstrap;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddSumiClasApplication(this IServiceCollection services)
    {
        var assembly = typeof(IUnitOfWork).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddUseCasesByConvention(assembly);

        return services;
    }
}
