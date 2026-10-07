using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MyTemplate.API.Extensions;
using MyTemplate.Application.DependencyInjection;
using MyTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Host.AddSerilogLogging();

builder.Services.AddApiConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=launchkit.db";

builder.Services.AddHealthChecks()
    .AddSqlite(connectionString, name: "sqlite");

var app = builder.Build();

await InfrastructureServices.SeedAdminUserAsync(app.Services);

app.UseApiConfiguration();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.Run();
