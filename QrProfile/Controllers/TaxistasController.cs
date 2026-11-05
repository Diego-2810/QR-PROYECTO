using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // <-- 1. Agregado para leer los secretos
using QrProfile.Models;
using QrProfile.Repositories;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace QrProfile.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/taxistas")]
public class TaxistasController : ControllerBase
{
    private readonly ITaxistaRepository _taxistaRepository;
    private readonly IConfiguration _configuration; // <-- 2. Variable para la configuración

    // 3. Inyectamos IConfiguration en el constructor junto con el repositorio
    public TaxistasController(ITaxistaRepository taxistaRepository, IConfiguration configuration)
    {
        _taxistaRepository = taxistaRepository;
        _configuration = configuration;
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetPerfil(Guid publicId)
    {
        var perfil = await _taxistaRepository.GetPublicoAsync(publicId);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarPerfil([FromForm] ActualizarTaxistaRequest modeloActual, IFormFile? ArchivoFoto) 
    {
        // Nota 2: la clase es [AllowAnonymous] (para el GET público), por eso validamos la sesión acá
        // e identificamos al taxista por su cuenta de Google, igual que en AdministracionController.
        var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (googleId is null)
        {
            return Unauthorized();
        }

        if (ArchivoFoto != null && ArchivoFoto.Length > 0)
        {
            var account = new Account(
                _configuration["Cloudinary:CloudName"],
                _configuration["Cloudinary:ApiKey"],
                _configuration["Cloudinary:ApiSecret"]
            );
            var cloudinary = new Cloudinary(account);

            using (var stream = ArchivoFoto.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(ArchivoFoto.FileName, stream),
                    Folder = "taxistas_profiles", 
                    Format = "jpg", // Convierte HEIC/HEIF (iPhone) y otros formatos a JPG, visible en cualquier navegador
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill") 
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null || uploadResult.SecureUrl == null)
                {
                    return BadRequest(uploadResult.Error?.Message ?? "No se pudo subir la imagen.");
                }
                
                // Guardamos la URL segura en la propiedad del modelo
                modeloActual.FotoUrl = uploadResult.SecureUrl.ToString();
            }
        }

        // --- LÓGICA DE BASE DE DATOS ---
        // Nota 1: si no se subió una foto nueva ni vino FotoUrl, conservamos la foto actual.
        if (string.IsNullOrWhiteSpace(modeloActual.FotoUrl))
        {
            var perfilActual = await _taxistaRepository.GetPropioAsync(googleId);
            if (perfilActual is null)
            {
                return NotFound();
            }
            modeloActual.FotoUrl = perfilActual.FotoUrl;
        }

        // Mismos Trim() que AdministracionController; los opcionales vacíos se guardan como '' (columnas NOT NULL DEFAULT '').
        // Las validaciones de ActualizarTaxistaRequest ya las aplicó [ApiController] antes de entrar al método.
        modeloActual.NombreCompleto = modeloActual.NombreCompleto.Trim();
        modeloActual.Telefono = modeloActual.Telefono.Trim();
        modeloActual.FotoUrl = modeloActual.FotoUrl?.Trim();
        modeloActual.Vehiculo = modeloActual.Vehiculo?.Trim() ?? string.Empty;
        modeloActual.Patente = modeloActual.Patente?.Trim() ?? string.Empty;
        modeloActual.DatosPago = modeloActual.DatosPago?.Trim() ?? string.Empty;
        modeloActual.Servicios = modeloActual.Servicios.Trim();

        var actualizado = await _taxistaRepository.ActualizarPropioAsync(googleId, modeloActual);
        if (!actualizado)
        {
            return NotFound();
        }
        
        return Ok(new { mensaje = "Perfil guardado con éxito", perfil = modeloActual });
    }
}