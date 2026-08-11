using MyTemplate.Domain.Entities;

namespace MyTemplate.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> CommitAsync();
}
