using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public static class DbSetExtensions
{
    public static async Task ExecuteWithRetryAsync(this DbContext context, Func<Task> operation,
        CancellationToken cancellationToken = default, int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
                {
                    await operation();
                    await context.SaveChangesAsync(cancellationToken);
                    if (context.Database.CurrentTransaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }

                    break; // Break from the loop on success
                }
            }
            catch (DbUpdateException)
            {
                if (attempt == maxRetries - 1)
                {
                    throw; // Re-throw the exception if max retries reached
                }
            }
        }
    }
}
