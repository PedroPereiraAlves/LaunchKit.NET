using Microsoft.EntityFrameworkCore;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;
using MyTemplate.Infrastructure.Context;

namespace MyTemplate.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<AuditLog> logs)
    {
        await _context.AuditLogs.AddRangeAsync(logs);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take)
        => await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .ToListAsync();

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId)
        => await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityName == entityName && x.EntityId == entityId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync();
}
