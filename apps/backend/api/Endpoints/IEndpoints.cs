namespace SumiClas.Api.Endpoints;

/// <summary>
/// Implemented by each module's endpoints class; <see cref="EndpointsExtensions.MapEndpoints"/> discovers and maps every implementation.
/// </summary>
public interface IEndpoints
{
    void Map(IEndpointRouteBuilder app);
}
