using CloudinaryDotNet;

namespace QrProfile.Services;

/// <summary>Cliente de Cloudinary compartido. Si faltan las credenciales, Configurado es false.</summary>
public sealed class CloudinaryProveedor
{
    public CloudinaryProveedor(IConfiguration configuration)
    {
        CloudName = configuration["Cloudinary:CloudName"] ?? string.Empty;
        ApiKey = configuration["Cloudinary:ApiKey"] ?? string.Empty;
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(CloudName) && !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(apiSecret))
        {
            Cliente = new Cloudinary(new Account(CloudName, ApiKey, apiSecret));
        }
    }

    public Cloudinary? Cliente { get; }
    public bool Configurado => Cliente is not null;
    public string CloudName { get; }
    public string ApiKey { get; }
}
