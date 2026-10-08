using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrProfile.Models;
using QrProfile.Repositories;

namespace QrProfile.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/taxistas")]
public class TaxistasController : ControllerBase
{
    private readonly ITaxistaRepository _taxistaRepository;

    public TaxistasController(ITaxistaRepository taxistaRepository)
    {
        _taxistaRepository = taxistaRepository;
    }

    [HttpGet("{taxistaId:int}")]
    public async Task<IActionResult> GetPerfil(int taxistaId)
    {
        var perfil = await _taxistaRepository.GetPublicoAsync(taxistaId);
        return perfil is null ? NotFound() : Ok(perfil);
    }
}