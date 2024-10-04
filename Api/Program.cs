using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;
using MinimalApi.Infraestructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Logging.AddConsole();

builder.Services.AddDbContext<Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("mysql");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        b => b.MigrationsAssembly("MinimalApi.Infraestructure")
    );
});

builder.Services.AddScoped<IAdministratorService, AdministratorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
    try
    {
        dbContext.Database.OpenConnection();
        Console.WriteLine("Database Connection successful!");
        dbContext.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Connection failed: {ex.Message}");
    }
}

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "admin" && loginDTO.Password == "admin")
        return Results.Ok("Login Success");
    else
        return Results.Unauthorized();
});

app.Run();
