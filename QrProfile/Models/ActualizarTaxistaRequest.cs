using System.ComponentModel.DataAnnotations;

namespace QrProfile.Models;

public class ActualizarTaxistaRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, StringLength(40, MinimumLength = 6), TelefonoInternacional]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? FotoUrl { get; set; }

    [StringLength(120)]
    public string? Vehiculo { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Patente { get; set; } = string.Empty;

    [StringLength(250)]
    public string? DatosPago { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 2)]
    public string Servicios { get; set; } = string.Empty;
}