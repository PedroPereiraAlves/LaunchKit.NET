using MyTemplate.Domain.Entities;

namespace MyTemplate.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddRangeAsync(IEnumerable<AuditLog> logs);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId);
}
