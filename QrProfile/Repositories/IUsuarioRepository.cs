using QrProfile.Models;

namespace QrProfile.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario> RegistrarOBuscarUsuarioGoogleAsync(Usuario usuario);
}