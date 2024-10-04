using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.ModelViews;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;
using MinimalApi.Infraestructure.Services;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Logging.AddConsole();

#region Builder Services
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
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

var app = builder.Build();
#endregion

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

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrators
app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    if (administratorService.Login(loginDTO) != null)
        return Results.Ok("Login Success");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");
#endregion

#region Vehicles
app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
    var vehicle = new Vehicle
    {
        Name = vehicleDTO.Name,
        Brand = vehicleDTO.Brand,
        Year = vehicleDTO.Year
    };
    vehicleService.Create(vehicle);

    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
}).WithTags("Veículos");

app.MapGet("/vehicles", ([FromQuery] int page, [FromQuery] string? name, [FromQuery] string? brand, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAll(page, name, brand);
    return Results.Ok(vehicles);
}).WithTags("Veículos");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
        return Results.NotFound();
    return Results.Ok(vehicle);
}).WithTags("Veículos");

app.MapPut("/vehicles/{id}", ([FromRoute] int id, [FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
        return Results.NotFound();

    vehicle.Name = vehicleDTO.Name;
    vehicle.Brand = vehicleDTO.Brand;
    vehicle.Year = vehicleDTO.Year;

    vehicleService.Update(vehicle);

    return Results.Ok(vehicle);
}).WithTags("Veículos");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
        return Results.NotFound();

    vehicleService.Delete(id);

    return Results.NoContent();
}).WithTags("Veículos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
