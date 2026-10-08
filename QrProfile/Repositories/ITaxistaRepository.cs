using QrProfile.Models;

namespace QrProfile.Repositories;

public interface ITaxistaRepository
{
    Task EnsurePerfilAsync(Usuario usuario);
    Task<TaxistaPerfil?> GetPublicoAsync(int taxistaId);
    Task<TaxistaPerfil?> GetPropioAsync(string googleId);
    Task<bool> ActualizarPropioAsync(string googleId, ActualizarTaxistaRequest perfil);
}