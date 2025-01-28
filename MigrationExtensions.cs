using Microsoft.EntityFrameworkCore;

namespace recipes_api;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using RecipeDbContext dbContext = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();

        dbContext.Database.Migrate();
    }
}