using MyTemplate.API.Extensions;
using MyTemplate.API.Middleware;
using MyTemplate.Application.DependencyInjection;
using MyTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();

builder.Services.AddApiConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=launchkit.db";

builder.Services.AddHealthChecks()
    .AddSqlite(connectionString, name: "sqlite");

var app = builder.Build();

await InfrastructureServices.SeedAdminUserAsync(app.Services);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseApiConfiguration(app.Environment);
app.MapHealthChecks("/health");

app.Run();
