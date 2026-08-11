using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Infrastructure.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(User.PasswordHash)
    };

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
            return;

        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;
        var userName = _currentUser.UserName ?? "system";
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy ??= userName;
                auditEntries.Add(CreateLog(entry.Entity, "Created", Snapshot(entry), userId, userName, now));
            }
            else if (entry.State is EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userName;
                auditEntries.Add(CreateLog(entry.Entity, "Updated", Snapshot(entry), userId, userName, now));
            }
            else if (entry.State is EntityState.Deleted)
            {
                auditEntries.Add(CreateLog(entry.Entity, "Deleted", Snapshot(entry), userId, userName, now));
            }
        }

        if (auditEntries.Count > 0)
            context.Set<AuditLog>().AddRange(auditEntries);
    }

    private static AuditLog CreateLog(
        BaseEntity entity,
        string action,
        string? changes,
        string? userId,
        string? userName,
        DateTime occurredAt)
        => new()
        {
            EntityName = entity.GetType().Name,
            EntityId = entity.Id.ToString(),
            Action = action,
            ChangesJson = changes,
            UserId = userId,
            UserName = userName,
            OccurredAt = occurredAt
        };

    private static string Snapshot(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (SensitiveProperties.Contains(property.Metadata.Name))
            {
                values[property.Metadata.Name] = "***";
                continue;
            }

            values[property.Metadata.Name] = entry.State == EntityState.Deleted
                ? property.OriginalValue
                : property.CurrentValue;
        }

        return JsonSerializer.Serialize(values);
    }
}
