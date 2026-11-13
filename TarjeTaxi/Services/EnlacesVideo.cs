using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using TarjeTaxi.Models;

namespace TarjeTaxi.Services;

/// <summary>
/// Reconoce links de video. YouTube se guarda normalizado para mostrarlo dentro de la tarjeta;
/// Instagram, TikTok, Facebook y Vimeo se guardan como "enlace" y se abren en su app.
/// Cualquier otro sitio se rechaza.
/// </summary>
public static partial class EnlacesVideo
{
    private static readonly string[] SitiosPermitidos =
        ["instagram.com", "tiktok.com", "facebook.com", "fb.watch", "vimeo.com"];

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$")]
    private static partial Regex IdYouTube();

    public static (string Tipo, string Url)? Analizar(string? valor)
    {
        if (!Uri.TryCreate(valor?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return null;
        }

        var host = uri.Host.ToLowerInvariant();
        foreach (var prefijo in new[] { "www.", "m.", "music.", "vm.", "web." })
        {
            if (host.StartsWith(prefijo)) { host = host[prefijo.Length..]; break; }
        }

        var segmentos = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? id = null;
        if (host == "youtu.be")
        {
            id = segmentos.FirstOrDefault();
        }
        else if (host == "youtube.com" || host == "youtube-nocookie.com")
        {
            if (segmentos.FirstOrDefault() == "watch")
            {
                id = QueryHelpers.ParseQuery(uri.Query).TryGetValue("v", out var v) ? v.ToString() : null;
            }
            else if (segmentos.Length >= 2 && segmentos[0] is "shorts" or "embed" or "live")
            {
                id = segmentos[1];
            }
        }

        if (id is not null)
        {
            return IdYouTube().IsMatch(id) ? (TiposMedio.YouTube, $"https://www.youtube.com/watch?v={id}") : null;
        }

        if (SitiosPermitidos.Any(sitio => host == sitio || host.EndsWith("." + sitio)))
        {
            var seguro = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = -1 };
            return (TiposMedio.Enlace, seguro.Uri.AbsoluteUri);
        }

        return null;
    }
}
