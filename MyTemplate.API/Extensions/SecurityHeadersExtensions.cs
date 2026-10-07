namespace MyTemplate.API.Extensions;

public static class SecurityHeadersExtensions
{
    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
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

            await next();
        });

        return app;
    }
}
