using AutoMapper;
using MediatR;
using MyTemplate.Application.DTOs;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Application.Features.Products.Queries;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id);
        return entity is null ? null : _mapper.Map<ProductDto>(entity);
    }
}
