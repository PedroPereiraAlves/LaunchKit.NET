using AutoMapper;
using MediatR;
using MyTemplate.Application.DTOs;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Application.Features.Products.Commands;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Product>();
        var entity = await repository.GetByIdAsync(request.Id);
        if (entity is null)
            return null;

        entity.Name = request.Name;
        entity.Quantity = request.Quantity;
        entity.Price = request.Price;

        repository.Update(entity);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<ProductDto>(entity);
    }
}
