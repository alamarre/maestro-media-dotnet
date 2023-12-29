using System.Linq.Expressions;
using Maestro.Database.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Maestro.Database;
public class Table<T> : ITable<T> where T : class
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly AsyncLocal<MediaDbContext> _contextHolder = new();

    private DbSet<T> _dbSet {
        get {
            return _context.Set<T>();
        }
    }

    private MediaDbContext _context {
        get {         
            if(_contextHolder.Value == null) {
                var scope = scopeFactory.CreateScope();
                _contextHolder.Value = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            }
            return _contextHolder.Value;
        }
    }

    public Table(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }


    async Task ITable<T>.DeleteAsync(T value)
    {
        _dbSet.Remove(value);
        await _context.SaveChangesAsync();
    }

    async Task ITable<T>.DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        _dbSet.RemoveRange(_dbSet.Where(predicate));
        await _context.SaveChangesAsync();
    }

    async Task<T?> ITable<T>.FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FindAsync(predicate);
    }

    async Task<T?> ITable<T>.GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).FirstOrDefaultAsync();
    }

    async Task<T> ITable<T>.GetOrCreateAsync(Expression<Func<T, bool>> predicate, Func<T, T> createTransform)
    {
        return await DbUtilities.GetOrCreateAsync(_context, _dbSet, predicate, createTransform);
    }

    async Task<T> ITable<T>.InsertAsync(T value)
    {
        var result = _dbSet.Add(value);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    async Task<IEnumerable<T>> ITable<T>.ListAsync(Expression<Func<T, bool>> predicate, int limit, T? bookmark)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    async Task<T> ITable<T>.UpdateAsync(T value)
    {
        var result = _dbSet.Update(value);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    async Task<T> ITable<T>.UpsertAsync(Expression<Func<T, bool>> predicate, Func<T, T> createOrUpdateTransform)
    {
         return await DbUtilities.UpsertAsync(_context, _dbSet, predicate, createOrUpdateTransform);
    }
}
