namespace SumiClas.Tests.Api;

using Microsoft.AspNetCore.Http;
using SumiClas.Tests.Shared;
using System.Text.Json;

public abstract class ApiUnitTestCase : UnitTestCase
{
    protected static async Task<(int Status, JsonElement Body)> ExecuteAsync(IResult result)
    {
        DefaultHttpContext context = HttpContextMother.WithProblemDetails();
        await result.ExecuteAsync(context);
        return (context.Response.StatusCode, await ReadBodyAsync(context));
    }

    protected static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(context.Response.Body);
        return document.RootElement.Clone();
    }
}
