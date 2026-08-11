using Microsoft.AspNetCore.Identity;
using MyTemplate.Application.Abstractions;
using MyTemplate.Domain.Entities;

namespace MyTemplate.Infrastructure.Auth;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(new User(), password);

    public bool Verify(string hash, string password)
    {
        var result = _hasher.VerifyHashedPassword(new User(), hash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
