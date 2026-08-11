namespace MyTemplate.Shared.Results;

/// <summary>
/// Envelope genérico reutilizável entre camadas (opcional).
/// A API também expõe <c>ApiResponse&lt;T&gt;</c> para respostas HTTP.
/// </summary>
public class Result<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }

    public static Result<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static Result<T> Fail(string message)
        => new() { Success = false, Message = message };
}
