using MediatR;
using MyTemplate.Application.DTOs;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Application.Features.Audit.Queries;

public class GetEntityHistoryHandler : IRequestHandler<GetEntityHistoryQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetEntityHistoryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetEntityHistoryQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetByEntityAsync(request.EntityName, request.EntityId);
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
