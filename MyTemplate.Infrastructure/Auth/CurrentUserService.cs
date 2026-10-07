using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        User?.FindFirstValue("name")
        ?? User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? User?.FindFirstValue(ClaimTypes.Email);

    public string? Role =>
        User?.FindFirstValue("role")
        ?? User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
