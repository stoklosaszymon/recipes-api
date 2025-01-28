using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace recipes_api;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllRecipes(string searchTerm);
    Task<Recipe> GetRecipeById(int id);
    Task<Recipe> AddRecipe(RecipeDto recipe);
    Task<Recipe> UpdateRecipe(RecipeUpdateDto recipe, int id);
    Task<Recipe> DeleteRecipe(int id);
}

public class RecipesRepository(RecipeDbContext context) : IRecipeRepository
{
    public async Task<IEnumerable<Recipe>> GetAllRecipes(string searchTerm)
    {
        var recipes = await context.Recipes.Where(recipe => recipe.Title.ToLower().Contains(searchTerm.ToLower()))
            .ToListAsync();
        return recipes;
    }

    public async Task<Recipe> GetRecipeById(int id)
    {
        return await context.Recipes.Where(recipe => recipe.Id == id)
            .Include(recipe => recipe.Ingredients).Select((recipe) => new Recipe
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                CookingTime = recipe.CookingTime,
                Image = recipe.Image,
                Ingredients = recipe.Ingredients,
                Steps = recipe.Steps
            })
            .FirstAsync();
    }

    public async Task<Recipe> AddRecipe(RecipeDto recipe)
    {
        Recipe newRecipe = new Recipe
        {
            Title = recipe.Title,
            Description = recipe.Description,
            CookingTime = recipe.CookingTime,
            Image = recipe.Image,
            Steps = recipe.Steps,
            Ingredients = recipe.Ingredients.Select(i => new Ingredient
            {
                Name = i.Name,
                Amount = i.Amount
            }).ToList(),
        };

        context.Recipes.Add(newRecipe);
        await context.SaveChangesAsync();

        return newRecipe;
    }

    public async Task<Recipe> DeleteRecipe(int id)
    {
        var recipe = await context.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            throw new KeyNotFoundException($"Recipe with id {id} not found.");
        }

        context.Recipes.Remove(recipe);
        await context.SaveChangesAsync();

        return recipe;
    }

    public async Task<Recipe> UpdateRecipe(RecipeUpdateDto toUpdate, int id)
    {
        var recipeToUpdate = await context.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipeToUpdate == null)
        {
            throw new KeyNotFoundException($"Recipe with id {id} not found.");
        }

        recipeToUpdate.Title = toUpdate.Title;
        recipeToUpdate.Description = toUpdate.Description;
        recipeToUpdate.CookingTime = toUpdate.CookingTime;
        recipeToUpdate.Image = toUpdate.Image;
        recipeToUpdate.Steps = toUpdate.Steps;

        context.Ingredients.RemoveRange(recipeToUpdate.Ingredients);

        recipeToUpdate.Ingredients = toUpdate.Ingredients.Select(i => new Ingredient
        {
            Name = i.Name,
            Amount = i.Amount
        }).ToList();

        await context.SaveChangesAsync();

        return recipeToUpdate;
    }
}