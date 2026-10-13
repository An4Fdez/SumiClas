namespace SumiClas.Tests.Api;

using Microsoft.Extensions.Configuration;

public static class ConfigurationMother
{
    public static IConfiguration Create(IReadOnlyDictionary<string, string?> settings)
        => new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
}
