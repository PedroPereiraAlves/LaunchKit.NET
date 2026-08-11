namespace MyTemplate.Application.Abstractions;

public interface IMetricsService
{
    Task<MetricsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public record MetricsSnapshot(
    int ProductsCount,
    int UsersCount,
    int AuditLogsCount,
    TimeSpan Uptime,
    DateTime TimestampUtc);
