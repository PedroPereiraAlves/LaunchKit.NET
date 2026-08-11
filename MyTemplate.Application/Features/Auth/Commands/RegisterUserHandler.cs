using MediatR;
using MyTemplate.Application.Abstractions;
using MyTemplate.Application.DTOs;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;
using MyTemplate.Shared.Auth;

namespace MyTemplate.Application.Features.Auth.Commands;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Email e senha são obrigatórios.");

        var existing = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.Email == email);

        if (existing.Any())
            throw new InvalidOperationException("Já existe um usuário com este email.");

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = Roles.User
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.CommitAsync();

        var token = _jwtTokenService.GenerateToken(user, out var expiresAt);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}
