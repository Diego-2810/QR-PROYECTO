using TarjeTaxi.Models;

namespace TarjeTaxi.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario> RegistrarOBuscarUsuarioGoogleAsync(Usuario usuario);
}