using System.ComponentModel.DataAnnotations;

namespace QrProfile.Models;

public class ActualizarTaxistaRequest
{
    /// <summary>Nombre o nombre comercial (ej. "Diego" o "Diego Ya").</summary>
    [Required(ErrorMessage = "Escribí tu nombre o el nombre con el que te conocen.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(59, ErrorMessage = "El apellido puede tener hasta 59 caracteres.")]
    public string? Apellido { get; set; }

    [Required, StringLength(40, MinimumLength = 6), TelefonoInternacional]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? FotoUrl { get; set; }

    [StringLength(50, ErrorMessage = "La marca puede tener hasta 50 caracteres.")]
    public string? Marca { get; set; }

    [StringLength(60, ErrorMessage = "El modelo puede tener hasta 60 caracteres.")]
    public string? Modelo { get; set; }

    [StringLength(30)]
    public string? Patente { get; set; } = string.Empty;

    [StringLength(250)]
    public string? DatosPago { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 2)]
    public string Servicios { get; set; } = string.Empty;

    /// <summary>"Nombre Apellido" (o solo el nombre). Se guarda en NombreCompleto para la tarjeta.</summary>
    public string NombreCompleto => Unir(Nombre, Apellido);

    /// <summary>"Marca Modelo". Se guarda en Vehiculo para la tarjeta.</summary>
    public string Vehiculo => Unir(Marca, Modelo);

    /// <summary>Quita espacios sobrantes y normaliza el teléfono. Los opcionales vacíos quedan en "".</summary>
    public void Normalizar()
    {
        Nombre = Nombre.Trim();
        Apellido = Apellido?.Trim() ?? string.Empty;
        // [TelefonoInternacional] ya validó el formato; acá se guarda normalizado (+5491112345678).
        Telefono = TelefonoInternacional.Normalizar(Telefono)!;
        FotoUrl = FotoUrl?.Trim();
        Marca = Marca?.Trim() ?? string.Empty;
        Modelo = Modelo?.Trim() ?? string.Empty;
        Patente = Patente?.Trim() ?? string.Empty;
        DatosPago = DatosPago?.Trim() ?? string.Empty;
        Servicios = Servicios.Trim();
    }

    private static string Unir(params string?[] partes) =>
        string.Join(' ', partes.Where(parte => !string.IsNullOrWhiteSpace(parte)).Select(parte => parte!.Trim()));
}
