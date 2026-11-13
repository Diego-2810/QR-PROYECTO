using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using TarjeTaxi.Models;
using TarjeTaxi.Repositories;

namespace TarjeTaxi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITaxistaRepository _taxistaRepository;

    public AuthController(IUsuarioRepository usuarioRepository, ITaxistaRepository taxistaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _taxistaRepository = taxistaRepository;
    }

    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleResponse))
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("id");
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(googleId) || string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Google no devolvió un identificador y email válidos.");
        }

        var usuario = new Usuario
        {
            GoogleId = googleId,
            Email = email,
            Nombre = User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue("name")
                ?? email,
            FotoUrl = User.FindFirstValue("picture")
        };

        var usuarioRegistrado = await _usuarioRepository.RegistrarOBuscarUsuarioGoogleAsync(usuario);
        await _taxistaRepository.EnsurePerfilAsync(usuarioRegistrado);
        return Redirect("/admin");
    }
}