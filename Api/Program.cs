using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.ModelViews;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enumerables;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;
using MinimalApi.Infraestructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

builder.Configuration
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Logging.AddConsole();

#region Builder Services
builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT: Bearer {seu_token}",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
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
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administrators
string CreateJwtToken(Administrator administrator)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Email, administrator.Email),
        new Claim("Perfil", administrator.Role)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

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
    var adm = administratorService.Login(loginDTO);
    if (adm != null)
    {
        string token = CreateJwtToken(adm);
        return Results.Ok(new LoggedAdm
        {
            Email = adm.Email,
            Role = Enum.Parse<Role>(adm.Role),
            Token = token,
        });
    } else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

app.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
{
    var validation = validateAdmDTO(administratorDTO);
    if (validation.Messages.Count > 0)
        return Results.BadRequest(validation);

    var adm = new Administrator
    {
        Email = administratorDTO.Email,
        Password = administratorDTO.Password,
        Role = administratorDTO.Role.ToString() ?? Role.Editor.ToString(),
    };
    administratorService.Create(adm);

    return Results.Created($"/administrator/{adm.Id}", new AdministratorModelView { 
        Id = adm.Id,
        Email = adm.Email,
        Role = Enum.Parse<Role>(adm.Role),
    });
}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administrators", ([FromQuery] int?page, IAdministratorService administratorService) =>
{
    var adms = new List<AdministratorModelView>();
    var administrators = administratorService.GetAll(page ?? 1);
    foreach(var administrator in administrators)
    {
        adms.Add(new AdministratorModelView { 
            Id = administrator.Id,
            Email = administrator.Email, 
            Role = Enum.Parse<Role>(administrator.Role),
        });
    }

    return Results.Ok(adms);
}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administrators/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
{
    var administrator = administratorService.GetById(id);
    if (administrator == null)
        return Results.NotFound();

    var adm = new AdministratorModelView
    {
        Id = administrator.Id,
        Email = administrator.Email,
        Role = Enum.Parse<Role>(administrator.Role),
    };

    return Results.Ok(adm);
}).RequireAuthorization().WithTags("Administradores");
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
}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/vehicles", ([FromQuery] int page, [FromQuery] string? name, [FromQuery] string? brand, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAll(page, name, brand);
    return Results.Ok(vehicles);
}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
        return Results.NotFound();
    return Results.Ok(vehicle);
}).RequireAuthorization().WithTags("Veículos");

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
}).RequireAuthorization().WithTags("Veículos");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
        return Results.NotFound();

    vehicleService.Delete(id);

    return Results.NoContent();
}).RequireAuthorization().WithTags("Veículos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
