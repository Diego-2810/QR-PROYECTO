using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TarjeTaxi.Models;

/// <summary>
/// Normaliza teléfonos al formato internacional E.164 (+ código de país + número, solo dígitos).
/// Es el formato que necesitan wa.me y tel: para funcionar desde cualquier país.
/// </summary>
public static partial class TelefonoInternacional
{
    public const string MensajeError =
        "Ingresá el teléfono con código de país, por ejemplo +54 9 11 1234 5678.";

    [GeneratedRegex(@"[\s\-().]")]
    private static partial Regex Separadores();

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex FormatoE164();

    /// <summary>
    /// Devuelve el teléfono como "+5491112345678", o null si no tiene código de país o no es válido.
    /// Acepta espacios, guiones, puntos y paréntesis, y "00" como reemplazo de "+".
    /// </summary>
    public static string? Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = Separadores().Replace(valor.Trim(), string.Empty);
        if (limpio.StartsWith("00"))
        {
            limpio = "+" + limpio[2..];
        }

        return FormatoE164().IsMatch(limpio) ? limpio : null;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class TelefonoInternacionalAttribute : ValidationAttribute
{
    public TelefonoInternacionalAttribute() : base(TelefonoInternacional.MensajeError) { }

    public override bool IsValid(object? value) =>
        value is null || TelefonoInternacional.Normalizar(value as string) is not null;
}
