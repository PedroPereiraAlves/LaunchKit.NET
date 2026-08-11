using MediatR;
using MyTemplate.Application.DTOs;

namespace MyTemplate.Application.Features.Audit.Queries;

public record GetEntityHistoryQuery(string EntityName, string EntityId) : IRequest<IEnumerable<AuditLogDto>>;
