using MediatR;
using MyTemplate.Application.DTOs;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Application.Features.Audit.Queries;

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take <= 0 ? 100 : request.Take;
        var logs = await _auditLogRepository.GetRecentAsync(take);
        return logs.Select(x => new AuditLogDto
        {
            Id = x.Id,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            Action = x.Action,
            ChangesJson = x.ChangesJson,
            UserId = x.UserId,
            UserName = x.UserName,
            OccurredAt = x.OccurredAt
        });
    }
}
