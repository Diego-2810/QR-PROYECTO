using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrProfile.Models;
using QrProfile.Repositories;

namespace QrProfile.Controllers;

[ApiController]
[Authorize]
[Route("api/administracion")]
public class AdministracionController : ControllerBase
{
    private readonly ITaxistaRepository _taxistaRepository;

    public AdministracionController(ITaxistaRepository taxistaRepository)
    {
        _taxistaRepository = taxistaRepository;
    }

    [HttpGet("perfil")]
    public async Task<IActionResult> GetPerfil()
    {
        var googleId = GetGoogleId();
        if (googleId is null)
        {
            return Unauthorized();
        }

        var perfil = await _taxistaRepository.GetPropioAsync(googleId);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarTaxistaRequest request)
    {
        var googleId = GetGoogleId();
        if (googleId is null)
        {
            return Unauthorized();
        }

        request.NombreCompleto = request.NombreCompleto.Trim();
        request.Telefono = request.Telefono.Trim();
        request.FotoUrl = request.FotoUrl?.Trim();
        request.Vehiculo = request.Vehiculo.Trim();
        request.Patente = request.Patente.Trim();
        request.DatosPago = request.DatosPago.Trim();
        request.Servicios = request.Servicios.Trim();

        var actualizado = await _taxistaRepository.ActualizarPropioAsync(googleId, request);
        return actualizado ? NoContent() : NotFound();
    }

    private string? GetGoogleId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
}