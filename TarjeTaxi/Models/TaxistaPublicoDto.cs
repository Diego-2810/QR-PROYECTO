namespace TarjeTaxi.Models;

public class TaxistaPublicoDto
{
    public Guid PublicId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string Vehiculo { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Patente { get; set; } = string.Empty;
    public string DatosPago { get; set; } = string.Empty;
    public string Servicios { get; set; } = string.Empty;
}

