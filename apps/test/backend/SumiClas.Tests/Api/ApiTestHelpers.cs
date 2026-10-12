using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SumiClas.Tests.Api;

internal static class ApiTestHelpers
{
    public static DefaultHttpContext NewHttpContext()
    {
        var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        return context;
    }

    public static async Task<(int Status, JsonElement Body)> ExecuteAsync(IResult result)
    {
        var context = NewHttpContext();
        await result.ExecuteAsync(context);
        return (context.Response.StatusCode, await ReadBodyAsync(context));
    }

    public static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return document.RootElement.Clone();
    }
}
