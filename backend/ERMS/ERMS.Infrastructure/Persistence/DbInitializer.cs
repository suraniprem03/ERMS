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

            // Safely add missing columns to food_claims table if they don't exist
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE food_claims ADD COLUMN IF NOT EXISTS food_date date NULL;
                ");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not alter food_claims table, it might already have the columns.");
            }

            // Safely add missing columns to travel_claims table if they don't exist
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE travel_claims ADD COLUMN IF NOT EXISTS mode_of_transport varchar(50) NULL;
                    ALTER TABLE travel_claims ADD COLUMN IF NOT EXISTS place_visited varchar(200) NULL;
                ");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not alter travel_claims table, it might already have the columns.");
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
