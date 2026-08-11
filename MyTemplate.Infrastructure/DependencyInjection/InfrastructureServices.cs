using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTemplate.Application.Abstractions;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;
using MyTemplate.Infrastructure.Auth;
using MyTemplate.Infrastructure.Context;
using MyTemplate.Infrastructure.Interceptors;
using MyTemplate.Infrastructure.Repositories;
using MyTemplate.Infrastructure.Services;
using MyTemplate.Shared.Auth;

namespace MyTemplate.Infrastructure.DependencyInjection;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IMetricsService, MetricsService>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IUnitOfWork, MyTemplate.Infrastructure.UnitOfWork.UnitOfWork>();

        return services;
    }

    public static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();

        await context.Database.EnsureCreatedAsync();

        const string adminEmail = "admin@launchkit.local";
        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        context.Users.Add(new User
        {
            Email = adminEmail,
            FullName = "LaunchKit Admin",
            Role = Roles.Admin,
            PasswordHash = passwordHasher.Hash("Admin@123")
        });

        await context.SaveChangesAsync();
    }
}
