using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QrProfile.Models;

/// <summary>Álbum de un viaje del taxista (ej. "Cataratas del Iguazú").</summary>
public class Viaje
{
    public int Id { get; set; }
    [JsonIgnore] public int TaxistaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public int Orden { get; set; }
    public List<ViajeMedio> Medios { get; set; } = new();
}

/// <summary>Foto, video o link de un viaje. Tipo: imagen | video | youtube | enlace.</summary>
public class ViajeMedio
{
    public int Id { get; set; }
    [JsonIgnore] public int ViajeId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    [JsonIgnore] public string? CloudinaryPublicId { get; set; }
    public int Orden { get; set; }
}

public static class TiposMedio
{
    public const string Imagen = "imagen";
    public const string Video = "video";
    public const string YouTube = "youtube";
    public const string Enlace = "enlace";
}

public static class LimitesViajes
{
    public const int MaxViajes = 30;
    public const int MaxMediosPorViaje = 20;
    public const long MaxBytesImagen = 10L * 1024 * 1024;
    public const long MaxBytesVideo = 100L * 1024 * 1024;
    public const int MaxSegundosVideo = 60;
}

public class GuardarViajeRequest
{
    [Required(ErrorMessage = "Escribí un título para el viaje.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El título debe tener entre 2 y 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción puede tener hasta 1000 caracteres.")]
    public string? Descripcion { get; set; }

    public DateTime? Fecha { get; set; }
}

public class FirmaSubidaRequest
{
    [Required, RegularExpression("^(imagen|video)$", ErrorMessage = "Tipo de archivo no válido.")]
    public string Tipo { get; set; } = string.Empty;
}

public class AgregarMedioSubidoRequest
{
    [Required, RegularExpression("^(imagen|video)$", ErrorMessage = "Tipo de archivo no válido.")]
    public string Tipo { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string CloudinaryPublicId { get; set; } = string.Empty;
}

public class AgregarEnlaceRequest
{
    [Required(ErrorMessage = "Pegá el link del video.")]
    [StringLength(2048)]
    public string Url { get; set; } = string.Empty;
}

public class OrdenRequest
{
    [Required, MinLength(1), MaxLength(100)]
    public List<int> Ids { get; set; } = new();
}

/// <summary>Taxista identificado por su cuenta: Id interno y PublicId (para la carpeta de Cloudinary).</summary>
public class TaxistaReferencia
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
}
