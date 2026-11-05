namespace QrProfile.Models;

public class TaxistaPublicoDto
{
    public string PublicId { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string Vehiculo { get; set; } = string.Empty;
    public string Patente { get; set; } = string.Empty;
    public string DatosPago { get; set; } = string.Empty;
    public string Servicios { get; set; } = string.Empty;
}
