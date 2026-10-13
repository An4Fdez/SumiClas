namespace SumiClas.Tests.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

public static class HttpContextMother
{
    public static DefaultHttpContext Create(IServiceProvider? services, IReadOnlyCollection<Claim> claims)
    {
        DefaultHttpContext context = new();

        if (services is not null)
        {
            context.RequestServices = services;
            context.Response.Body = new MemoryStream();
        }

        if (claims.Count > 0)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        }

        return context;
    }

    public static DefaultHttpContext Empty() => Create(null, []);

    public static DefaultHttpContext WithClaim(string type, string value) => Create(null, [new Claim(type, value)]);

    public static DefaultHttpContext WithSubject(string subject) => WithClaim("sub", subject);

    public static DefaultHttpContext WithProblemDetails()
        => Create(new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider(), []);
}
