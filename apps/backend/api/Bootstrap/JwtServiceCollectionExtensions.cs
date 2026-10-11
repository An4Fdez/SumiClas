using System.Text;
using SumiClas.Api.Http;
using SumiClas.RefreshTokens.Domain;
using SumiClas.RefreshTokens.Infrastructure;

namespace SumiClas.Api.Bootstrap;

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
            .Validate(o => o.AccessTokenMinutes is >= 1 and <= 60, "Jwt:AccessTokenMinutes debe estar entre 1 y 60.")
            .Validate(o => o.RefreshTokenDays is >= 1 and <= 90, "Jwt:RefreshTokenDays debe estar entre 1 y 90.")
            .Validate(o => o.RefreshGraceSeconds is >= 0 and <= 60, "Jwt:RefreshGraceSeconds debe estar entre 0 y 60.")
            .ValidateOnStart();
        services.AddOptions<RefreshCookieOptions>()
            .Bind(configuration.GetSection(RefreshCookieOptions.SectionName));

        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();

        return services;
    }
}
