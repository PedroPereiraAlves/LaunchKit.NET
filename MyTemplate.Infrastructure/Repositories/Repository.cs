using Microsoft.EntityFrameworkCore;
using MyTemplate.Domain.Entities;
using MyTemplate.Domain.Interfaces;
using MyTemplate.Infrastructure.Context;
using System.Linq.Expressions;

namespace MyTemplate.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) 
        => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() 
        => await _dbSet.ToListAsync();
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
    public void Update(T entity) 
        => _dbSet.Update(entity);
    public void Remove(T entity) 
        => _dbSet.Remove(entity);
}
