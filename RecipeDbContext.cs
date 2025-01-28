using Microsoft.EntityFrameworkCore;

namespace recipes_api;

public class RecipeDbContext: DbContext
{
    public RecipeDbContext(DbContextOptions<RecipeDbContext> options): base(options)
    { }
    
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}