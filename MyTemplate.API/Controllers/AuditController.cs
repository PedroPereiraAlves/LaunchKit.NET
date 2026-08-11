using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTemplate.API.Responses;
using MyTemplate.Application.DTOs;
using MyTemplate.Application.Features.Audit.Queries;
using MyTemplate.Shared.Auth;

namespace MyTemplate.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(take), cancellationToken);
        return Ok(new ApiResponse<IEnumerable<AuditLogDto>>(true, "Logs de auditoria", result));
    }

    [HttpGet("{entityName}/{entityId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityHistory(string entityName, string entityId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEntityHistoryQuery(entityName, entityId), cancellationToken);
        return Ok(new ApiResponse<IEnumerable<AuditLogDto>>(true, "Histórico da entidade", result));
    }
}
