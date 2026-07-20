using ERMS.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERMS.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbInitializer");

        try
        {
            var dbContext = services.GetRequiredService<ERMSDbContext>();

            await dbContext.Database.MigrateAsync();

            // Safely add missing columns to expense_categories table if they don't exist
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE expense_categories ADD COLUMN IF NOT EXISTS max_limit numeric(18,2) NULL;
                    ALTER TABLE expense_categories ADD COLUMN IF NOT EXISTS is_dynamic boolean NOT NULL DEFAULT false;
                ");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not alter expense_categories table, it might already have the columns.");
            }

            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while initializing the database.");
            throw;
        }
    }
}
