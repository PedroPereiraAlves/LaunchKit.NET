using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using MyTemplate.API.ExceptionHandling;
using MyTemplate.API.Responses;
using Serilog;

namespace MyTemplate.API.Extensions;

public static class ApiConfiguration
{
    public const string CorsPolicyName = "Default";
    public const string AuthRateLimitPolicy = "auth";

    public static IServiceCollection AddApiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(pair => pair.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "Valor inválido."
                                : error.ErrorMessage)
                            .ToArray());

                var response = new ApiResponse<object>(false, "Dados inválidos.")
                {
                    Errors = errors,
                    TraceId = context.HttpContext.TraceIdentifier
                };

                return new BadRequestObjectResult(response);
            };
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "LaunchKit.NET API",
                Version = "v1",
                Description = "Template CRUD com CQRS, JWT, validação, auditoria e health checks"
            });
            options.AddSwaggerJwt();
        });

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
                    .Where(origin => !string.IsNullOrWhiteSpace(origin))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray() ?? [];

                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                    return;
                }

                if (environment.IsDevelopment())
                {
                    policy.SetIsOriginAllowed(origin =>
                            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                                || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    return;
                }

                policy.SetIsOriginAllowed(_ => false);
            });
        });

        var permitLimit = Math.Max(1, configuration.GetValue("AuthRateLimit:PermitLimit", 20));
        var windowSeconds = Math.Max(1, configuration.GetValue("AuthRateLimit:WindowSeconds", 60));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                if (httpContext.Response.HasStarted)
                    return;

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>(
                    false,
                    "Muitas tentativas. Aguarde um instante e tente novamente.")
                {
                    TraceId = httpContext.TraceIdentifier
                }, cancellationToken);
            };
        });

        return services;
    }

    public static WebApplication UseApiConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseSecurityHeaders();
        app.UseStaticFiles();
        app.UseCors(CorsPolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.MapGet("/dashboard", () => Results.Redirect("/dashboard/index.html"));

        return app;
    }
}
