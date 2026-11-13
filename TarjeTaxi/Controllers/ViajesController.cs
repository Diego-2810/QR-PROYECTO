using System.Globalization;
using System.Net;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarjeTaxi.Models;
using TarjeTaxi.Repositories;
using TarjeTaxi.Services;

namespace TarjeTaxi.Controllers;

/// <summary>
/// Sección "Mis viajes" del panel. Las fotos y videos se suben directo del navegador a Cloudinary
/// con una firma que da este controlador; después el navegador avisa qué subió y el servidor lo
/// verifica contra Cloudinary antes de guardarlo.
/// </summary>
[ApiController]
[Authorize]
[Route("api/viajes")]
public class ViajesController : ControllerBase
{
    private readonly IViajeRepository _viajes;
    private readonly CloudinaryProveedor _cloudinary;
    private readonly ILogger<ViajesController> _logger;

    public ViajesController(IViajeRepository viajes, CloudinaryProveedor cloudinary, ILogger<ViajesController> logger)
    {
        _viajes = viajes;
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var taxista = await ObtenerTaxistaAsync();
        return taxista is null ? Unauthorized() : Ok(await _viajes.ListarAsync(taxista.Id));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarViajeRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        if (await _viajes.ContarAsync(taxista.Id) >= LimitesViajes.MaxViajes)
        {
            return Error(HttpStatusCode.Conflict, $"Llegaste al máximo de {LimitesViajes.MaxViajes} viajes. Borrá alguno para agregar otro.");
        }

        Limpiar(request);
        var id = await _viajes.CrearAsync(taxista.Id, request);
        return Ok(await _viajes.ObtenerAsync(taxista.Id, id));
    }

    [HttpPut("{viajeId:int}")]
    public async Task<IActionResult> Actualizar(int viajeId, [FromBody] GuardarViajeRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        Limpiar(request);
        return await _viajes.ActualizarAsync(taxista.Id, viajeId, request) ? NoContent() : NotFound();
    }

    [HttpDelete("{viajeId:int}")]
    public async Task<IActionResult> Eliminar(int viajeId)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        var viaje = await _viajes.ObtenerAsync(taxista.Id, viajeId);
        if (viaje is null) return NotFound();

        await _viajes.EliminarAsync(taxista.Id, viajeId);
        foreach (var medio in viaje.Medios)
        {
            await BorrarDeCloudinaryAsync(medio.CloudinaryPublicId, medio.Tipo);
        }

        return NoContent();
    }

    [HttpPut("orden")]
    public async Task<IActionResult> Reordenar([FromBody] OrdenRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        return await _viajes.ReordenarAsync(taxista.Id, request.Ids) ? NoContent() : Error(HttpStatusCode.BadRequest, "El orden enviado no coincide con tus viajes. Recargá la página.");
    }

    /// <summary>Firma una subida directa a Cloudinary con un nombre de archivo que decide el servidor.</summary>
    [HttpPost("firma")]
    public async Task<IActionResult> FirmarSubida([FromBody] FirmaSubidaRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        if (_cloudinary.Cliente is null) return CloudinaryNoConfigurado();

        var publicId = $"{CarpetaDe(taxista)}{Guid.NewGuid():N}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var firma = _cloudinary.Cliente.Api.SignParameters(new SortedDictionary<string, object>
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        });
        var tipoRecurso = request.Tipo == TiposMedio.Video ? "video" : "image";

        return Ok(new
        {
            uploadUrl = $"https://api.cloudinary.com/v1_1/{_cloudinary.CloudName}/{tipoRecurso}/upload",
            apiKey = _cloudinary.ApiKey,
            timestamp,
            signature = firma,
            publicId,
            maxBytes = request.Tipo == TiposMedio.Video ? LimitesViajes.MaxBytesVideo : LimitesViajes.MaxBytesImagen,
            maxSegundosVideo = LimitesViajes.MaxSegundosVideo
        });
    }

    /// <summary>Registra un archivo ya subido a Cloudinary, después de verificarlo.</summary>
    [HttpPost("{viajeId:int}/medios")]
    public async Task<IActionResult> AgregarMedioSubido(int viajeId, [FromBody] AgregarMedioSubidoRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        if (_cloudinary.Cliente is null) return CloudinaryNoConfigurado();

        // Solo se aceptan archivos de la carpeta de este taxista (el nombre lo firmó el servidor).
        if (!request.CloudinaryPublicId.StartsWith(CarpetaDe(taxista), StringComparison.Ordinal))
        {
            return Error(HttpStatusCode.BadRequest, "El archivo no corresponde a tu cuenta.");
        }

        var viaje = await _viajes.ObtenerAsync(taxista.Id, viajeId);
        if (viaje is null)
        {
            await BorrarDeCloudinaryAsync(request.CloudinaryPublicId, request.Tipo);
            return NotFound();
        }

        var esVideo = request.Tipo == TiposMedio.Video;
        var recurso = await _cloudinary.Cliente.GetResourceAsync(new GetResourceParams(request.CloudinaryPublicId)
        {
            ResourceType = esVideo ? ResourceType.Video : ResourceType.Image
        });
        if (recurso.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(recurso.SecureUrl))
        {
            return Error(HttpStatusCode.BadRequest, "No se encontró el archivo subido. Probá subirlo de nuevo.");
        }

        string? rechazo = null;
        if (viaje.Medios.Count >= LimitesViajes.MaxMediosPorViaje)
        {
            rechazo = $"Cada viaje puede tener hasta {LimitesViajes.MaxMediosPorViaje} fotos y videos.";
        }
        else if (recurso.Bytes > (esVideo ? LimitesViajes.MaxBytesVideo : LimitesViajes.MaxBytesImagen))
        {
            rechazo = esVideo ? "El video supera los 100 MB." : "La foto supera los 10 MB.";
        }
        else if (esVideo && DuracionSegundos(recurso) > LimitesViajes.MaxSegundosVideo + 1)
        {
            rechazo = $"El video dura más de {LimitesViajes.MaxSegundosVideo} segundos. Para videos largos usá un link de YouTube.";
        }

        if (rechazo is not null)
        {
            await BorrarDeCloudinaryAsync(request.CloudinaryPublicId, request.Tipo);
            return Error(HttpStatusCode.BadRequest, rechazo);
        }

        await _viajes.AgregarMedioAsync(viajeId, request.Tipo, recurso.SecureUrl, request.CloudinaryPublicId);
        return Ok(await _viajes.ObtenerAsync(taxista.Id, viajeId));
    }

    [HttpPost("{viajeId:int}/enlaces")]
    public async Task<IActionResult> AgregarEnlace(int viajeId, [FromBody] AgregarEnlaceRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        var viaje = await _viajes.ObtenerAsync(taxista.Id, viajeId);
        if (viaje is null) return NotFound();
        if (viaje.Medios.Count >= LimitesViajes.MaxMediosPorViaje)
        {
            return Error(HttpStatusCode.Conflict, $"Cada viaje puede tener hasta {LimitesViajes.MaxMediosPorViaje} fotos y videos.");
        }

        var enlace = EnlacesVideo.Analizar(request.Url);
        if (enlace is null)
        {
            return Error(HttpStatusCode.BadRequest, "Link no válido. Se aceptan videos de YouTube, Instagram, TikTok, Facebook o Vimeo.");
        }

        await _viajes.AgregarMedioAsync(viajeId, enlace.Value.Tipo, enlace.Value.Url, null);
        return Ok(await _viajes.ObtenerAsync(taxista.Id, viajeId));
    }

    [HttpDelete("medios/{medioId:int}")]
    public async Task<IActionResult> EliminarMedio(int medioId)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        var medio = await _viajes.ObtenerMedioAsync(taxista.Id, medioId);
        if (medio is null) return NotFound();

        await _viajes.EliminarMedioAsync(medioId);
        await BorrarDeCloudinaryAsync(medio.CloudinaryPublicId, medio.Tipo);
        return NoContent();
    }

    [HttpPut("{viajeId:int}/medios/orden")]
    public async Task<IActionResult> ReordenarMedios(int viajeId, [FromBody] OrdenRequest request)
    {
        var taxista = await ObtenerTaxistaAsync();
        if (taxista is null) return Unauthorized();
        return await _viajes.ReordenarMediosAsync(taxista.Id, viajeId, request.Ids) ? NoContent() : Error(HttpStatusCode.BadRequest, "El orden enviado no coincide con las fotos del viaje. Recargá la página.");
    }

    // ---------- Auxiliares ----------

    private async Task<TaxistaReferencia?> ObtenerTaxistaAsync()
    {
        var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return googleId is null ? null : await _viajes.ObtenerTaxistaAsync(googleId);
    }

    private static string CarpetaDe(TaxistaReferencia taxista) => $"tarjetaxi/viajes/{taxista.PublicId:N}/";

    private static void Limpiar(GuardarViajeRequest request)
    {
        request.Titulo = request.Titulo.Trim();
        request.Descripcion = request.Descripcion?.Trim() ?? string.Empty;
    }

    private static double DuracionSegundos(GetResourceResult recurso)
    {
        var duracion = recurso.JsonObj?["duration"];
        return duracion is null ? 0 : double.TryParse(duracion.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var segundos) ? segundos : 0;
    }

    private async Task BorrarDeCloudinaryAsync(string? publicId, string tipo)
    {
        if (string.IsNullOrWhiteSpace(publicId) || _cloudinary.Cliente is null) return;
        try
        {
            await _cloudinary.Cliente.DestroyAsync(new DeletionParams(publicId)
            {
                ResourceType = tipo == TiposMedio.Video ? ResourceType.Video : ResourceType.Image,
                Invalidate = true
            });
        }
        catch (Exception exception)
        {
            // No frena al taxista: el archivo queda huérfano en Cloudinary y se puede borrar a mano.
            _logger.LogWarning(exception, "No se pudo borrar {PublicId} de Cloudinary.", publicId);
        }
    }

    private ObjectResult Error(HttpStatusCode estado, string mensaje) =>
        Problem(statusCode: (int)estado, title: mensaje);

    private ObjectResult CloudinaryNoConfigurado() =>
        Error(HttpStatusCode.ServiceUnavailable, "La subida de fotos y videos todavía no está configurada en el servidor (faltan las claves de Cloudinary).");
}
