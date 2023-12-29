using System.Linq.Expressions;
using Maestro.Database.Utilities;
using Microsoft.EntityFrameworkCore;

public static class DbSetExtensions {


    public static async Task<T> GetOrCreateAsync<T>(this DbSet<T> dbSet, DbContext context, Expression<Func<T, bool>> predicate, Func<T, T> createTransform) where T : class {
        return await DbUtilities.GetOrCreateAsync<T>(context, dbSet, predicate, createTransform);
    }

    public static async Task<T> UpsertAsync<T>(this DbSet<T> dbSet, DbContext context, Expression<Func<T, bool>> predicate, Func<T, T> createOrUpdateTransform) where T : class {
        return await DbUtilities.UpsertAsync(context, dbSet, predicate, createOrUpdateTransform);
    }

     public static async Task ExecuteWithRetryAsync(this DbContext context, Func<Task> operation, CancellationToken cancellationToken = default, int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
                {
                    await operation();
                    await transaction.CommitAsync(cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                    break; // Break from the loop on success
                }
            }
            catch (DbUpdateException)
            {
                if (attempt == maxRetries - 1) {
                    throw; // Re-throw the exception if max retries reached
                }
            }
        }
    }
}