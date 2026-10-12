using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SumiClas.Api.Http;
using SumiClas.Sessions.Infrastructure;

namespace SumiClas.Api.Configuration;

/// <summary>Configures the bearer scheme from the validated <see cref="JwtOptions"/>, so the key is only read once it has been checked.</summary>
public sealed class ConfigureJwtBearerOptions(IOptions<JwtOptions> jwtOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string AccessTokenType = "at+jwt";

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        var jwt = jwtOptions.Value;

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            RequireSignedTokens = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidTypes = [AccessTokenType],
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
                await ResultExtensions.UnauthenticatedProblem().ExecuteAsync(context.HttpContext);
            },
            OnForbidden = context => ResultExtensions.ForbiddenProblem().ExecuteAsync(context.HttpContext),
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);
}
