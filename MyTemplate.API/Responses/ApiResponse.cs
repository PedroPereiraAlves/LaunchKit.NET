using System.Text.Json.Serialization;

namespace MyTemplate.API.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    public ApiResponse(bool success, string? message = null, T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
