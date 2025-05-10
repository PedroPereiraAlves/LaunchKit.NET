using AutoMapper;
using MediatR;
using MyTemplate.Application.DTOs;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Application.Features.Products.Commands;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Product>(request);
        await _unitOfWork.Repository<Product>().AddAsync(entity);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<ProductDto>(entity);
    }
}
