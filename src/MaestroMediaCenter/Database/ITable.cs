using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Database;
public interface ITable<T> where T : class {

    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

    Task<T> GetOrCreateAsync(Expression<Func<T, bool>> predicate, Func<T, T> createTransform);

    Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate, int limit = 100, T? bookmark = null);
    Task<T> UpdateAsync(T value);

    Task<T> UpsertAsync(Expression<Func<T, bool>> predicate, Func<T, T> createOrUpdateTransform);

    Task<T> InsertAsync(T value);

    Task DeleteAsync(T value);

    Task DeleteAsync(Expression<Func<T, bool>> predicate);

}