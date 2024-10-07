using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.ModelViews;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enumerables;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;
using MinimalApi.Infraestructure.Services;
using System.Xml.Linq;

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
ValidationErrors validateAdmDTO(AdministratorDTO administratorDTO)
{
    var validation = new ValidationErrors();

    if (string.IsNullOrEmpty(administratorDTO.Email))
        validation.Messages.Add("The e-mail cannot be empty");

    if (string.IsNullOrEmpty(administratorDTO.Password))
        validation.Messages.Add("The password cannot be empty");

    if (administratorDTO.Role == null)
        validation.Messages.Add("The role cannot be empty");

    return validation;
}

app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    if (administratorService.Login(loginDTO) != null)
        return Results.Ok("Login Success");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

app.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
{
    var validation = validateAdmDTO(administratorDTO);
    if (validation.Messages.Count > 0)
        return Results.BadRequest(validation);

    var adm = new Administrator
    {
        Email = administratorDTO.Email,
        Password = administratorDTO.Password,
        Role = administratorDTO.Role.ToString() ?? Role.editor.ToString(),
    };
    administratorService.Create(adm);

    return Results.Created($"/administrator/{adm.Id}", adm);
}).WithTags("Administradores");

app.MapGet("/administrators", ([FromQuery] int? page, IAdministratorService administratorService) =>
{
    var administrators = administratorService.GetAll(page ?? 1);
    return Results.Ok(administrators);
}).WithTags("Administrators");

app.MapGet("/administrators/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
{
    var administrator = administratorService.GetById(id);
    if (administrator == null)
        return Results.NotFound();
    return Results.Ok(administrator);
}).WithTags("Veículos");
#endregion

#region Vehicles
ValidationErrors validateVehicleDTO(VehicleDTO vehicleDTO)
{
    var validation = new ValidationErrors();

    if (string.IsNullOrEmpty(vehicleDTO.Name))
        validation.Messages.Add("The name cannot be empty");

    if (string.IsNullOrEmpty(vehicleDTO.Brand))
        validation.Messages.Add("The brand cannot be empty");

    if (vehicleDTO.Year < 1950)
        validation.Messages.Add("Vehicle is too old, only years above 1950 are accepted");

    return validation;
}

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
    var validation = validateVehicleDTO(vehicleDTO);
    if(validation.Messages.Count > 0)
        return Results.BadRequest(validation);

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

    var validation = validateVehicleDTO(vehicleDTO);
    if (validation.Messages.Count > 0)
        return Results.BadRequest(validation);

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
