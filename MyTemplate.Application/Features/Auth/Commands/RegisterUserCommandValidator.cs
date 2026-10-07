using FluentValidation;

namespace MyTemplate.Application.Features.Auth.Commands;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .MaximumLength(256).WithMessage("O email deve ter no máximo 256 caracteres.")
            .EmailAddress().WithMessage("Informe um email válido.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .MaximumLength(128).WithMessage("A senha deve ter no máximo 128 caracteres.")
            .Matches("[A-Za-z]").WithMessage("A senha deve conter ao menos uma letra.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}
