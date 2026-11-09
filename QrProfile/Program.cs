using QrProfile.Repositories;
using QrProfile.Services;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

var builder = WebApplication.CreateBuilder(args);
var googleAuth = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleAuth.GetValue<string>("ClientId");
var googleClientSecret = googleAuth.GetValue<string>("ClientSecret");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection. Configurala en appsettings, User Secrets o mediante ConnectionStrings__DefaultConnection.");
}

try
{
    var connectionSettings = new MySqlConnectionStringBuilder(connectionString);
    if (string.IsNullOrWhiteSpace(connectionSettings.Server) || string.IsNullOrWhiteSpace(connectionSettings.Database))
    {
        throw new InvalidOperationException("La cadena MariaDB debe incluir Server y Database.");
    }
}
catch (Exception exception) when (exception is ArgumentException or FormatException or MySqlException)
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection no tiene un formato válido para MariaDB/MySQL.",
        exception);
}

if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
{
    if (builder.Environment.IsDevelopment())
    {
        Console.Error.WriteLine("Faltan credenciales de Google OAuth. En Development, configurá User Secrets para este proyecto:");
        Console.Error.WriteLine("  dotnet user-secrets set \"Authentication:Google:ClientId\" \"TU_CLIENT_ID\" --project QrProfile/QrProfile.csproj");
        Console.Error.WriteLine("  dotnet user-secrets set \"Authentication:Google:ClientSecret\" \"TU_CLIENT_SECRET\" --project QrProfile/QrProfile.csproj");
        Console.Error.WriteLine("WebApplication.CreateBuilder carga User Secrets automáticamente en Development cuando el proyecto tiene UserSecretsId.");
    }

    throw new InvalidOperationException(
        "Configure Authentication:Google:ClientId y Authentication:Google:ClientSecret mediante appsettings, User Secrets o las variables Authentication__Google__ClientId y Authentication__Google__ClientSecret antes de iniciar la API.");
}

var trustForwardedHeaders = builder.Configuration.GetValue<bool>("ForwardedHeaders:TrustProxyHeaders");
if (trustForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITaxistaRepository, TaxistaRepository>();

builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".QrProfile.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.ClaimActions.Add(new JsonKeyClaimAction(
            "picture",
            System.Security.Claims.ClaimValueTypes.String,
            "picture"));
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var taxistaRepository = scope.ServiceProvider.GetRequiredService<ITaxistaRepository>();
    var filasActualizadas = await taxistaRepository.RellenarPublicIdsFaltantesAsync();
    app.Logger.LogInformation(
        "Verificación inicial de PublicId completada. Filas actualizadas: {FilasActualizadas}.",
        filasActualizadas);
}

if (trustForwardedHeaders)
{
    app.UseForwardedHeaders();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

var publicPagePath = Path.Combine(app.Environment.WebRootPath!, "index.html");
var homePagePath = Path.Combine(app.Environment.ContentRootPath, "Views", "inicio.html");
var adminPagePath = Path.Combine(app.Environment.ContentRootPath, "Views", "administracion.html");

// "no-cache" obliga al navegador a revalidar el HTML, así después de cada despliegue
// nadie se queda con una versión vieja de las páginas.
static IResult PaginaHtml(HttpContext context, string path)
{
    context.Response.Headers.CacheControl = "no-cache";
    return Results.File(path, "text/html; charset=utf-8");
}

app.MapGet("/", (HttpContext context) => PaginaHtml(context, homePagePath));
app.MapGet("/perfil/{publicId:guid}", (HttpContext context) => PaginaHtml(context, publicPagePath));
app.MapGet("/admin", (HttpContext context) => PaginaHtml(context, adminPagePath))
    .RequireAuthorization();
app.MapGet("/panel", (HttpContext context) => PaginaHtml(context, adminPagePath))
    .RequireAuthorization();

// Endpoint de la API
app.MapGet("/api/profile/{lang}", async (string lang, IProfileService profileService) =>
{
    var profile = await profileService.GetProfileByLanguageAsync(lang);
    return profile is not null ? Results.Ok(profile) : Results.NotFound("Profile not found for this language.");
});

app.MapControllers();
    
app.Run();