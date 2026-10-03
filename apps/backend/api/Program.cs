using Asp.Versioning;
using SumiClas.Api.Bootstrap;
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
builder.Services.AddSumiClasIdentity();
builder.Services.AddSumiClasInfrastructure(builder.Configuration);
builder.Services.AddSumiClasApplication();


var app = builder.Build();
app.ApplySumiClasMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();
}

app.UseSwaggerUI(swaggerUiOptions =>
{
    if (app.DescribeApiVersions().Reverse().Any())
    {
        foreach (var apiVersion in app.DescribeApiVersions().Reverse())
        {
            swaggerUiOptions.SwaggerEndpoint($"/swagger/{apiVersion.GroupName}.json",
                apiVersion.GroupName.ToUpperInvariant());
        }
        
    }

});

app.MapHealthChecks("/healthy");
app.MapEndpoints();
app.UseHttpsRedirection();
app.Run();