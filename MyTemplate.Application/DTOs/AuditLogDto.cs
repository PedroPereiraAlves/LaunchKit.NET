namespace MyTemplate.Application.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? ChangesJson { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime OccurredAt { get; set; }
}
