using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarjeTaxi.Models;
using TarjeTaxi.Repositories;
using TarjeTaxi.Services;

namespace TarjeTaxi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/taxistas")]
public class TaxistasController : ControllerBase
{
    private const long MaxBytesFoto = 10L * 1024 * 1024;
    // Margen para el resto de los campos del formulario.
    private const long MaxBytesFormulario = MaxBytesFoto + 1024 * 1024;

    private readonly ITaxistaRepository _taxistaRepository;
    private readonly CloudinaryProveedor _cloudinary;

    public TaxistasController(ITaxistaRepository taxistaRepository, CloudinaryProveedor cloudinary)
    {
        _taxistaRepository = taxistaRepository;
        _cloudinary = cloudinary;
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetPerfil(Guid publicId)
    {
        var perfil = await _taxistaRepository.GetPublicoAsync(publicId);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    /// <summary>Viajes públicos del taxista, para la sección "Viajes" de la tarjeta.</summary>
    [HttpGet("{publicId:guid}/viajes")]
    public async Task<IActionResult> GetViajes(Guid publicId, [FromServices] IViajeRepository viajes)
    {
        var lista = await viajes.ListarPublicoAsync(publicId);
        return lista is null ? NotFound() : Ok(lista);
    }

    /// <summary>Guarda el perfil desde el panel, con foto opcional.</summary>
    [HttpPost]
    [RequestSizeLimit(MaxBytesFormulario)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytesFormulario)]
    public async Task<IActionResult> GuardarPerfil([FromForm] ActualizarTaxistaRequest modeloActual, IFormFile? ArchivoFoto)
    {
        // La clase es [AllowAnonymous] (para el GET público), por eso la sesión se valida acá
        // y el taxista se identifica por su cuenta de Google, igual que en AdministracionController.
        var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (googleId is null)
        {
            return Unauthorized();
        }

        var perfilActual = await _taxistaRepository.GetPropioAsync(googleId);
        if (perfilActual is null)
        {
            return NotFound();
        }

        if (ArchivoFoto is { Length: > 0 })
        {
            var rechazo = ValidarFoto(ArchivoFoto);
            if (rechazo is not null)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: rechazo);
            }

            if (_cloudinary.Cliente is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "La subida de fotos todavía no está configurada en el servidor. Guardá sin foto o avisá al administrador.");
            }

            await using var stream = ArchivoFoto.OpenReadStream();
            var resultado = await _cloudinary.Cliente.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(ArchivoFoto.FileName, stream),
                // Un archivo fijo por taxista: cada foto nueva reemplaza a la anterior en Cloudinary
                // (no se acumulan fotos viejas). La URL cambia de versión, así nadie ve la foto vieja.
                PublicId = $"taxistas_profiles/{perfilActual.PublicId:N}",
                Overwrite = true,
                Invalidate = true,
                Format = "jpg", // Convierte HEIC/HEIF (iPhone) y otros formatos a JPG
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
            });

            if (resultado.Error is not null || resultado.SecureUrl is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "No se pudo procesar la foto. Probá con otra imagen.");
            }

            modeloActual.FotoUrl = resultado.SecureUrl.ToString();
        }
        else if (string.IsNullOrWhiteSpace(modeloActual.FotoUrl))
        {
            // Sin foto nueva ni FotoUrl: se conserva la actual.
            modeloActual.FotoUrl = perfilActual.FotoUrl;
        }

        // Las validaciones de ActualizarTaxistaRequest ya las aplicó [ApiController] antes de entrar al método.
        modeloActual.Normalizar();

        var actualizado = await _taxistaRepository.ActualizarPropioAsync(googleId, modeloActual);
        return actualizado ? Ok(new { mensaje = "Perfil guardado con éxito" }) : NotFound();
    }

    private static string? ValidarFoto(IFormFile archivo)
    {
        if (archivo.Length > MaxBytesFoto)
        {
            return "La foto supera los 10 MB. Elegí una más liviana.";
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var esImagen = archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || extension is ".heic" or ".heif";
        return esImagen ? null : "El archivo elegido no es una imagen.";
    }
}
