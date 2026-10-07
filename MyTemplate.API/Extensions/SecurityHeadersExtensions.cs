namespace MyTemplate.API.Extensions;

public static class SecurityHeadersExtensions
{
    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            Apply(context);
            await next();
        });

        return app;
    }

    public static void Apply(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-Frame-Options"] = "DENY";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";

        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
        }
    }
}

public static class CorsOriginRules
{
    public static bool IsAllowed(string? origin, IConfiguration configuration, IHostEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return false;

        var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        if (configured.Length > 0)
            return configured.Contains(origin, StringComparer.OrdinalIgnoreCase);

        if (!environment.IsDevelopment())
            return false;

        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase));
    }

    public static void Apply(HttpContext context, IConfiguration configuration, IHostEnvironment environment)
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (!IsAllowed(origin, configuration, environment))
            return;

        context.Response.Headers["Access-Control-Allow-Origin"] = origin;
        context.Response.Headers["Vary"] = "Origin";
    }
}
