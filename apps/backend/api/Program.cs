using Asp.Versioning;
using SumiClas.Api.Configuration;
using SumiClas.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddApiVersioning(versionOptions =>
{
    versionOptions.AssumeDefaultVersionWhenUnspecified = true;
    versionOptions.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
    versionOptions.DefaultApiVersion = new ApiVersion(1, 0);
}).AddApiExplorer(explorerOptions => { explorerOptions.GroupNameFormat = "'v'VVV"; }).AddOpenApi();

builder.Services.AddHealthChecks();
builder.Services.AddValidation();
builder.Services.AddSumiClasInfrastructure(builder.Configuration);
builder.Services.AddSumiClasApplication();
builder.Services.AddSumiClasApiSecurity(builder.Configuration);
builder.Services.AddSumiClasRateLimiting(builder.Configuration);


var app = builder.Build();
app.ApplySumiClasMigrations();

// Configure the HTTP request pipeline.
// HTTPS only: nothing listens on plain HTTP, so there is no HTTP-to-HTTPS redirection to configure.

if (app.Environment.IsDevelopment())
{
    // The document needs a Bearer token even in Development: nothing is anonymous just because it is a dev environment.
    app.MapOpenApi().WithDocumentPerVersion().RequireAuthorization();
}

app.UseSwaggerUI(swaggerUiOptions =>
{
    if (app.DescribeApiVersions().Reverse().Any())
    {
        foreach (var apiVersion in app.DescribeApiVersions().Reverse())
        {
            swaggerUiOptions.SwaggerEndpoint($"/openapi/{apiVersion.GroupName}.json",
                apiVersion.GroupName.ToUpperInvariant());
        }

    }

});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/healthy").AllowAnonymous();
app.MapEndpoints();
app.Run();
