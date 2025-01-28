using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using recipes_api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions((options) =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IRecipeRepository, RecipesRepository>();
builder.Services.AddCors((options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());
}));

builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.ApplyMigrations();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();