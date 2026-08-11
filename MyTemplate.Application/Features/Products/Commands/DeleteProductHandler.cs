using MediatR;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;

namespace MyTemplate.Application.Features.Products.Commands;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Product>();
        var entity = await repository.GetByIdAsync(request.Id);
        if (entity is null)
            return false;

        repository.Remove(entity);
        await _unitOfWork.CommitAsync();
        return true;
    }
}
