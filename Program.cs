using MinimalAPI.Domain.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if(loginDTO.Username == "admin" && loginDTO.Password == "admin")
        return Results.Ok("Login Success");
    else
        return Results.Unauthorized();
});

app.Run();
