using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;
using MyTemplate.Infrastructure.Context;
using MyTemplate.Infrastructure.Repositories;

namespace MyTemplate.Infrastructure.UnitOfWorkName;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<T>(_context);
        }
        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> CommitAsync()
        => await _context.SaveChangesAsync();

    public void Dispose() 
        => _context.Dispose();
}
