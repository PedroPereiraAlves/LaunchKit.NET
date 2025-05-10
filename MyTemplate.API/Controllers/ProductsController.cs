using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTemplate.API.Responses;
using MyTemplate.Application.DTOs;
using MyTemplate.Application.Features.Products.Commands;
using MyTemplate.Application.Features.Products.Queries;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        return Ok(new ApiResponse<IEnumerable<ProductDto>>(true, "Produtos encontrados", result));
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return Created("", new ApiResponse<ProductDto>(true, "Produto criado com sucesso", result));
    }
}
