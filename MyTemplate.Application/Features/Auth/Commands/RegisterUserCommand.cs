using MediatR;
using MyTemplate.Application.DTOs;

namespace MyTemplate.Application.Features.Auth.Commands;

public class RegisterUserCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
