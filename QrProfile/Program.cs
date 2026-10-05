using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using QrProfile.Models;
using QrProfile.Repositories;
using QrProfile.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar el repositorio y el servicio en el contenedor de DI
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint de la API
app.MapGet("/api/profile/{lang}", async (string lang, IProfileService profileService) =>
{
    var profile = await profileService.GetProfileByLanguageAsync(lang);
    return profile is not null ? Results.Ok(profile) : Results.NotFound("Profile not found for this language.");
});

// Auth Endpoints
app.MapPost("/api/auth/register", async (UserRegisterDto dto, IUserRepository userRepository) =>
{
    var existingUser = await userRepository.GetUserByEmailAsync(dto.Email);
    if (existingUser != null)
    {
        return Results.BadRequest("El usuario ya existe.");
    }

    var user = new User
    {
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Slug = dto.Slug
    };

    var result = await userRepository.CreateUserAsync(user);
    return result ? Results.Ok() : Results.BadRequest("Error al crear el usuario.");
});

app.MapPost("/api/auth/login", async (UserLoginDto dto, IUserRepository userRepository, IConfiguration config) =>
{
    var user = await userRepository.GetUserByEmailAsync(dto.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
    {
        return Results.BadRequest("Credenciales inválidas.");
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    // Use a default key if not provided in appsettings.json to avoid runtime crashes during this demo
    var keyString = config["JwtSettings:Secret"] ?? "ClaveSuperSecretaParaDesarrolloLocal_ReemplazarEnProd!";
    var key = Encoding.ASCII.GetBytes(keyString);
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("Slug", user.Slug)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
});

app.Run();