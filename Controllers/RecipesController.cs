using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace recipes_api.Controllers;

[ApiController]
[Route("[controller]")]
public class RecipesController(IRecipeRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecipeBasic>>> GetRecipes([FromQuery] string searchTerm = "")
    {
        IEnumerable<Recipe> recipes = await Task.Run(() => repository.GetAllRecipes(searchTerm));
        return Ok(recipes.Select(r => new RecipeBasic
        {
            Id = r.Id,
            Description = r.Description,
            CookingTime = r.CookingTime,
            Image = r.Image,
            Title = r.Title
        }).ToList());
    }

    [HttpGet]
    [Route("docker/test")]
    public IActionResult DockerTest()
    {
        return Ok("test");
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Recipe>> GetRecipeById(int id)
    {
        System.Console.WriteLine(id);
        return Ok(await repository.GetRecipeById(id));
    }

    [HttpPost]
    public async Task<ActionResult<Recipe>> AddRecipe([FromBody] RecipeDto recipe)
    {
        try
        {
            var newRecipe = await repository.AddRecipe(recipe);
            return Ok(newRecipe);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }
    
    [HttpPost]
    [Route("{id}/edit")]
    public async Task<ActionResult<Recipe>> EditRecipe(int id, [FromBody] RecipeUpdateDto recipe)
    {
        try
        {
            var newRecipe = await repository.UpdateRecipe(recipe, id);
            return Ok(newRecipe);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }
    
    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult<Recipe>> DeleteRecipe(int id)
    {
        var recipe = await repository.DeleteRecipe(id);
        return Ok(recipe); 
    }
    
    [HttpGet]
    [Route("fromUrl")]
    public async Task<ActionResult<Recipe>> RecipeFromUrl([FromQuery] string url)
    {
        using HttpClient client = new HttpClient();
        try
        {
            string urlToFetch = "https://api.supadata.ai/v1/youtube/transcript?url=" + url;
            client.DefaultRequestHeaders.Add("x-api-key", Environment.GetEnvironmentVariable("YT_READER_KEY"));
            
            HttpResponseMessage response = await client.GetAsync(urlToFetch);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var prompt = "To jest transkrypcja filmu na youtube z procesu gotowania potrawy. " + responseBody.ToString() +
                         " wyciag z niego nazwe potrawy, okresl ile trwa jej ugotowanie jakie sa składniki i proces tworzenia," +
                         " wynik zwroć jako prawidłowy format json {Title: string; Description: string; CookingTime: number; Image: string; Steps: string[]; Ingredients: Ingredient[ { amount: string, name: string }];}  (Jeśli nie znasz ilości jakiegoś składniku wstaw '')" +
                         ". Zwróc tylko JSON nic więcej, Kazde pole musi być podane. Tytuł jest wymagany. Jezeli CookingTime nie jest podany postaraj się oszacować ( w minutach ).";
            var googleAi = new GoogleAI(apiKey: Environment.GetEnvironmentVariable("GEMINI_KEY"));
            var model = googleAi.GenerativeModel("gemini-1.5-flash",
                new GenerationConfig { ResponseMimeType = "application/json" });
            var aiResponse = await model.GenerateContent(prompt);

            var recipe = await repository.AddRecipe(JsonConvert.DeserializeObject<RecipeDto>(aiResponse.Text));

            return Ok(recipe);
        }
        catch (HttpRequestException e)
        {
            return BadRequest($"Request error: {e.Message}");
        }
    }
}