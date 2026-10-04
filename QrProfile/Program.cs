using QrProfile.Repositories;
using QrProfile.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar el repositorio y el servicio en el contenedor de DI
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();

var app = builder.Build();

// Habilitar el servicio de archivos estáticos (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint de la API
app.MapGet("/api/profile/{lang}", async (string lang, IProfileService profileService) =>
{
    var profile = await profileService.GetProfileByLanguageAsync(lang);
    return profile is not null ? Results.Ok(profile) : Results.NotFound("Profile not found for this language.");
});

app.Run();