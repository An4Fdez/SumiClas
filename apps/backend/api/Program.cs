using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using SumiClas.Api.Configuration;
using SumiClas.Api.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// log4net is the only provider (console and rolling file, see log4net.config); the Logging section of appsettings
// still filters by category before a message reaches it.
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net();

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
builder.Services.AddSumiClasValidation();
builder.Services.AddSumiClasInfrastructure(builder.Configuration);
builder.Services.AddSumiClasApplication();
builder.Services.AddSumiClasAiOptions(builder.Configuration);
builder.Services.AddSumiClasWeeklySummaryWriter();
builder.Services.AddSumiClasApiSecurity(builder.Configuration);
builder.Services.AddSumiClasRateLimiting(builder.Configuration);
builder.Services.AddSumiClasExceptionHandling();


WebApplication app = builder.Build();
app.ApplySumiClasMigrations();

// First in the pipeline, so it also catches what comes out of the rest of the middleware and of the endpoints.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
// HTTPS only: nothing listens on plain HTTP, so there is no HTTP-to-HTTPS redirection to configure.

if (app.Environment.IsDevelopment())
{
    // Only the document is anonymous, so Swagger UI can load it; the endpoints it describes still require a Bearer token.
    app.MapOpenApi().WithDocumentPerVersion().AllowAnonymous();
}

app.UseSwaggerUI(swaggerUiOptions =>
{
    if (app.DescribeApiVersions().Reverse().Any())
    {
        foreach (ApiVersionDescription apiVersion in app.DescribeApiVersions().Reverse())
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
