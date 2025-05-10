using MediatR;
using MyTemplate.Application.DTOs;

namespace MyTemplate.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>> { }
