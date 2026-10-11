using System.Text;
using Microsoft.Extensions.Options;

namespace SumiClas.Authentication.Application;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public const int MinimumSigningKeyBytes = 32;

    public const string MissingSigningKeyMessage =
        "Falta la clave de firma de los tokens (Jwt:SigningKey). Defínela con " +
        "dotnet user-secrets set \"Jwt:SigningKey\" \"<clave de al menos 32 caracteres>\" " +
        "o con la variable de entorno Jwt__SigningKey.";

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            failures.Add(MissingSigningKeyMessage);
        }
        else if (Encoding.UTF8.GetByteCount(options.SigningKey) < MinimumSigningKeyBytes)
        {
            failures.Add($"Jwt:SigningKey debe tener al menos {MinimumSigningKeyBytes} bytes (32 caracteres). " + MissingSigningKeyMessage);
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Jwt:Issuer es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Jwt:Audience es obligatorio.");
        }

        if (options.AccessTokenMinutes is < 1 or > 60)
        {
            failures.Add("Jwt:AccessTokenMinutes debe estar entre 1 y 60.");
        }

        if (options.RefreshTokenDays is < 1 or > 90)
        {
            failures.Add("Jwt:RefreshTokenDays debe estar entre 1 y 90.");
        }

        if (options.RefreshGraceSeconds is < 0 or > 60)
        {
            failures.Add("Jwt:RefreshGraceSeconds debe estar entre 0 y 60.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
