using FluentValidation;

namespace MyTemplate.Application.Features.Products.Commands;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O id é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade não pode ser negativa.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser negativo.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("O preço deve ter no máximo 18 dígitos e 2 casas decimais.");
    }
}
