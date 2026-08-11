using MyTemplate.Application.Abstractions;
using MyTemplate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MyTemplate.Infrastructure.Services;

public class MetricsService : IMetricsService
{
    private static readonly DateTime StartedAtUtc = DateTime.UtcNow;
    private readonly AppDbContext _context;

    public MetricsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MetricsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products.CountAsync(cancellationToken);
        var users = await _context.Users.CountAsync(cancellationToken);
        var audits = await _context.AuditLogs.CountAsync(cancellationToken);

        return new MetricsSnapshot(
            products,
            users,
            audits,
            DateTime.UtcNow - StartedAtUtc,
            DateTime.UtcNow);
    }
}
