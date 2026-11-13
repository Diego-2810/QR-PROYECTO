using TarjeTaxi.Models;

namespace TarjeTaxi.Repositories;

public interface ITaxistaRepository
{
    Task<int> RellenarPublicIdsFaltantesAsync();
    Task EnsurePerfilAsync(Usuario usuario);
    Task<TaxistaPublicoDto?> GetPublicoAsync(Guid publicId);
    Task<TaxistaPerfil?> GetPropioAsync(string googleId);
    Task<bool> ActualizarPropioAsync(string googleId, ActualizarTaxistaRequest perfil);
}