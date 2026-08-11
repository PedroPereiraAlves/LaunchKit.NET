using MyTemplate.Domain.Entities;

namespace MyTemplate.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}
