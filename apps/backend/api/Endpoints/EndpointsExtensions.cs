namespace SumiClas.Api.Endpoints;

public static class EndpointsExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointTypes = typeof(IEndpoints).Assembly.GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false }
            && typeof(IEndpoints).IsAssignableFrom(type));

        foreach (var endpointType in endpointTypes)
        {
            ((IEndpoints)Activator.CreateInstance(endpointType)!).Map(app);
        }

        return app;
    }
}
