using MediatR;

namespace MyTemplate.Application.Features.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
