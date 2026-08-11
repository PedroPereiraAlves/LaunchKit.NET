using MyTemplate.API.Extensions;
using MyTemplate.API.Middleware;
using MyTemplate.Application.DependencyInjection;
using MyTemplate.Infrastructure.Context;
using MyTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();

builder.Services.AddApiConfiguration();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseApiConfiguration(app.Environment);

app.Run();
