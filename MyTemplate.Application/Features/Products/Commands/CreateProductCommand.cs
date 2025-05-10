using MediatR;
using MyTemplate.Application.DTOs;

namespace MyTemplate.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
