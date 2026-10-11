using System.Text;
using SumiClas.Api.Http;
using SumiClas.Sessions.Domain;
using SumiClas.Sessions.Infrastructure;

namespace SumiClas.Api.Configuration;

public static class JwtServiceCollectionExtensions
{
    private const int MinimumSigningKeyBytes = 32;

    private const string SigningKeyHelp =
        "Defínela con dotnet user-secrets set \"Jwt:SigningKey\" \"<clave de al menos 32 caracteres>\" " +
        "o con la variable de entorno Jwt__SigningKey.";

    public static IServiceCollection AddSumiClasJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.SigningKey),
                $"Falta la clave de firma de los tokens (Jwt:SigningKey). {SigningKeyHelp}")
            .Validate(
                o => string.IsNullOrWhiteSpace(o.SigningKey) || Encoding.UTF8.GetByteCount(o.SigningKey) >= MinimumSigningKeyBytes,
                $"Jwt:SigningKey debe tener al menos {MinimumSigningKeyBytes} bytes (32 caracteres). {SigningKeyHelp}")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer es obligatorio.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience es obligatorio.")
            .ValidateOnStart();
        services.AddOptions<RefreshCookieOptions>()
            .Bind(configuration.GetSection(RefreshCookieOptions.SectionName));

        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();

        return services;
    }
}
