
namespace recipes_api;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;

    // Foreign key to the Recipe table
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}

public class Recipe
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? CookingTime { get; set; }
    public string? Image { get; set; }
    public List<string>? Steps { get; set; }
    public List<Ingredient> Ingredients { get; set; }
}

public class RecipeBasic
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? CookingTime { get; set; }
    public string? Image { get; set; }
}

public class RecipeDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int CookingTime { get; set; }
    public string Image { get; set; }
    public List<IngredientDto> Ingredients { get; set; }
    public List<string> Steps { get; set; }
}
public class IngredientDto
{
    public string Name { get; set; }
    public string Amount { get; set; }
}

public class IngredientUpdateDto : IngredientDto
{
    public int Id { get; set; }
}

public class RecipeUpdateDto : RecipeDto
{
    public new List<IngredientUpdateDto> Ingredients { get; set; }
}