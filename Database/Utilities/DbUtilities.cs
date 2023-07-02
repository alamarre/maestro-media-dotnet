using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Maestro.Database.Utilities;
public static class DbUtilities {

     public static async Task<T> GetOrCreateAsync<T>(
        PostgresDbContext context, 
        Func<Task<T>> getQuery, 
        Func<Task> createQueries) {
        return await GetOrCreateAsync(context, getQuery, async () => {
            await createQueries();
            return await getQuery();
        });
     }
    public static async Task<T> GetOrCreateAsync<T>(
        PostgresDbContext context, 
        Func<Task<T>> getQuery, 
        Func<Task<T>> createQueries)
    {
        // First, try to fetch the entity
        var entity = await getQuery();

        if (entity != null)
        {
            return entity;
        }
        
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Create a new entity using the createQueries delegate
            entity = await createQueries();

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return entity;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            // Check if the exception is caused by the unique constraint violation
            if (IsUniqueConstraintViolation(ex))
            {
                // Fetch the entity again
                entity = await getQuery();

                if (entity != null)
                {
                    return entity;
                }
            }
            else
            {
                throw new InvalidOperationException("An error occurred while creating the entity or getting the entity by query.", ex);
            }
        }
        
        return default!;
    }

     public static async Task<T> GetOrCreateAsync<T>(
        PostgresDbContext context, 
        DbSet<T> dbSet,
        Expression<Func<T, bool>> getQuery, 
        Func<T, T> createOrUpdateTransform)
        where T : class
    {
        // First, try to fetch the entity
        var entity = await dbSet.FindAsync(getQuery);

        if (entity != null)
        {
            return entity;
        }
        
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            T input = Activator.CreateInstance<T>();;
            T saveResult = createOrUpdateTransform(input);
            var entry = await dbSet.AddAsync(saveResult);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return entry.Entity;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            // Check if the exception is caused by the unique constraint violation
            if (IsUniqueConstraintViolation(ex))
            {
                // Fetch the entity again
                entity = await dbSet.FindAsync(getQuery!);

                if (entity != null)
                {
                    return entity;
                }
            }
            else
            {
                throw new InvalidOperationException("An error occurred while creating the entity or getting the entity by query.", ex);
            }
        }
        
        return default!;
    }


    public static async Task<T> UpsertAsync<T>(
        PostgresDbContext context, 
        DbSet<T> dbSet,
        Expression<Func<T, bool>> getQuery, 
        Func<T, T> createOrUpdateTransform)
        where T : class
    {
        // First, try to fetch the entity
        T? entity = await dbSet.FindAsync(getQuery);

        if (entity != null)
        {
            T updated = createOrUpdateTransform(entity);
            var result = dbSet.Update(updated);
            await context.SaveChangesAsync();


            if(!result.Entity.Equals(entity)) {
                // track change potentially
            }
            return result.Entity;
        }
        
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            T input = Activator.CreateInstance<T>();
            T saveResult = createOrUpdateTransform(input);
            var entry = await dbSet.AddAsync(saveResult);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            entity = entry.Entity;
            // record creation
            return entity;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            // Check if the exception is caused by the unique constraint violation
            if (IsUniqueConstraintViolation(ex))
            {
                // Fetch the entity again
                entity = await dbSet.FindAsync(getQuery);

                if (entity != null)
                {
                    var updated = createOrUpdateTransform(entity);
                    var result = dbSet.Update(updated);
                    await context.SaveChangesAsync();
                    if(!result.Entity.Equals(entity)) {
                        // track change potentially
                    }
                    return updated;
                }
            }
            else
            {
                throw new InvalidOperationException("An error occurred while creating the entity or getting the entity by query.", ex);
            }
        }
        
        return default!;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // For PostgreSQL
        if (ex.InnerException is PostgresException pgEx && (pgEx.SqlState == "23505" || pgEx.SqlState == "23000"))
        {
            return true;
        }

        // For other database providers, add appropriate checks
        return false;
    }
}
