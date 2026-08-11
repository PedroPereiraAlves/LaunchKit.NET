using MediatR;
using MyTemplate.Application.DTOs;

namespace MyTemplate.Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
