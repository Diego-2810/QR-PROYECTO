using TarjeTaxi.Repositories;
using TarjeTaxi.Services;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
        Console.Error.WriteLine("  dotnet user-secrets set \"Authentication:Google:ClientId\" \"TU_CLIENT_ID\" --project TarjeTaxi/TarjeTaxi.csproj");
        Console.Error.WriteLine("  dotnet user-secrets set \"Authentication:Google:ClientSecret\" \"TU_CLIENT_SECRET\" --project TarjeTaxi/TarjeTaxi.csproj");
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

// Claves de la cookie de sesión guardadas en la base (ver MySqlXmlRepository).
builder.Services.AddDataProtection().SetApplicationName("TaxiYa");
builder.Services.Configure<KeyManagementOptions>(options =>
    options.XmlRepository = new MySqlXmlRepository(connectionString));

// /health: Render lo consulta para saber si la app está viva y conectada a la base.
builder.Services.AddHealthChecks().AddAsyncCheck("base-de-datos", async cancellationToken =>
{
    try
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
    catch (Exception exception)
    {
        return HealthCheckResult.Unhealthy("Sin conexión a la base de datos.", exception);
    }
});

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITaxistaRepository, TaxistaRepository>();
builder.Services.AddScoped<IViajeRepository, ViajeRepository>();
builder.Services.AddSingleton<CloudinaryProveedor>();

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

await MySqlXmlRepository.VerificarTablaAsync(connectionString);
await ViajeRepository.VerificarTablasAsync(connectionString);

var cloudinary = app.Configuration.GetSection("Cloudinary");
if (new[] { "CloudName", "ApiKey", "ApiSecret" }.Any(clave => string.IsNullOrWhiteSpace(cloudinary[clave])))
{
    app.Logger.LogWarning(
        "Cloudinary no está configurado (Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret). La app funciona, pero los taxistas no van a poder subir fotos.");
}

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
// Dirección vieja del panel: se mantiene como redirección por si alguien la guardó.
app.MapGet("/panel", () => Results.Redirect("/admin"));

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();