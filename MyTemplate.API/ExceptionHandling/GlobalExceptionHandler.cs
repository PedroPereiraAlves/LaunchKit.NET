using Microsoft.AspNetCore.Diagnostics;
using MyTemplate.API.Extensions;
using MyTemplate.API.Responses;
using MyTemplate.Application.Exceptions;

namespace MyTemplate.API.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted || httpContext.RequestAborted.IsCancellationRequested)
            return httpContext.RequestAborted.IsCancellationRequested;

        var (status, message, errors) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
        else
            _logger.LogInformation(
                "Request rejected with {StatusCode}. TraceId: {TraceId}. {Message}",
                status,
                httpContext.TraceIdentifier,
                message);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json";
        SecurityHeadersExtensions.Apply(httpContext);
        CorsOriginRules.Apply(httpContext, _configuration, _environment);

        var response = new ApiResponse<object>(false, message)
        {
            Errors = errors is { Count: > 0 } ? errors : null,
            TraceId = httpContext.TraceIdentifier
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private (int Status, string Message, IDictionary<string, string[]>? Errors) Map(Exception exception)
    {
        return exception switch
        {
            ValidationFailedException validation =>
                (StatusCodes.Status400BadRequest, validation.Message, validation.Errors),
            ConflictException conflict =>
                (StatusCodes.Status409Conflict, conflict.Message, null),
            UnauthorizedAccessException unauthorized =>
                (StatusCodes.Status401Unauthorized, string.IsNullOrWhiteSpace(unauthorized.Message) ? "Não autorizado." : unauthorized.Message, null),
            KeyNotFoundException notFound =>
                (StatusCodes.Status404NotFound, string.IsNullOrWhiteSpace(notFound.Message) ? "Recurso não encontrado." : notFound.Message, null),
            _ => (StatusCodes.Status500InternalServerError, ServerErrorMessage(exception), null)
        };
    }

    private string ServerErrorMessage(Exception exception)
        => _environment.IsDevelopment() && !string.IsNullOrWhiteSpace(exception.Message)
            ? exception.Message
            : "Erro interno no servidor.";
}
