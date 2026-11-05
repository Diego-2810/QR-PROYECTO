using QrProfile.Models;

namespace QrProfile.Repositories;

public interface ITaxistaRepository
{
    Task EnsurePerfilAsync(Usuario usuario);
    Task<TaxistaPublicoDto?> GetPublicoAsync(Guid publicId);
    Task<TaxistaPerfil?> GetPropioAsync(string googleId);
    Task<bool> ActualizarPropioAsync(string googleId, ActualizarTaxistaRequest perfil);
}