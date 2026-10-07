using FluentValidation;

namespace MyTemplate.Application.Features.Auth.Commands;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .MaximumLength(256).WithMessage("O email deve ter no máximo 256 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MaximumLength(128).WithMessage("A senha deve ter no máximo 128 caracteres.");
    }
}
