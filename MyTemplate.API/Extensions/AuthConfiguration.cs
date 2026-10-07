using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyTemplate.API.Responses;

namespace MyTemplate.API.Extensions;

public static class AuthConfiguration
{
    public const string DevelopmentSigningKey = "LaunchKit.NET-Dev-Secret-Key-Change-In-Production-32+";

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Jwt:Issuer e Jwt:Audience são obrigatórios.");

        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException(
                "Jwt:Key deve ter pelo menos 32 bytes. Defina Jwt__Key (variável de ambiente) ou User Secrets fora dos arquivos versionados.");

        if (!environment.IsDevelopment() && string.Equals(key, DevelopmentSigningKey, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "A chave JWT de desenvolvimento não pode ser usada fora do ambiente Development.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        if (context.Response.HasStarted)
                            return;

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers.WWWAuthenticate = "Bearer";
                        context.Response.ContentType = "application/json";

                        var message = context.AuthenticateFailure is null
                            ? "Autenticação necessária."
                            : "Token inválido ou expirado.";

                        await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, message)
                        {
                            TraceId = context.HttpContext.TraceIdentifier
                        });
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                            return;

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, "Permissão insuficiente.")
                        {
                            TraceId = context.HttpContext.TraceIdentifier
                        });
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static void AddSwaggerJwt(this Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Informe o token JWT. Exemplo: eyJhbGciOi..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
