using MediatR;
using MyTemplate.Application.DTOs;
using System.Text.Json.Serialization;

namespace MyTemplate.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<ProductDto?>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
