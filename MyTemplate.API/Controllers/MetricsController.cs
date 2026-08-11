using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTemplate.API.Responses;
using MyTemplate.Application.Abstractions;
using MyTemplate.Shared.Auth;

namespace MyTemplate.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;

    public MetricsController(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var snapshot = await _metricsService.GetSnapshotAsync(cancellationToken);
        return Ok(new ApiResponse<object>(true, "Métricas da aplicação", new
        {
            snapshot.ProductsCount,
            snapshot.UsersCount,
            snapshot.AuditLogsCount,
            UptimeSeconds = (int)snapshot.Uptime.TotalSeconds,
            snapshot.TimestampUtc
        }));
    }
}
