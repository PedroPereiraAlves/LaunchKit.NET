using MediatR;
using MyTemplate.Application.DTOs;

namespace MyTemplate.Application.Features.Audit.Queries;

public record GetAuditLogsQuery(int Take = 100) : IRequest<IEnumerable<AuditLogDto>>;
