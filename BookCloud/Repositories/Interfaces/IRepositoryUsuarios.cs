using BookCloud.Models;

namespace BookCloud.Repositories.Interfaces
{
    public interface IRepositoryUsuarios
    {
        Task CreateUserASync(Usuario user, UsuarioSeguridad seguridad);
        Task<Usuario> GetUserByEmail(string email);
        Task<Usuario> GetInfoUsario(string id);
        Task ActualizarUsuarioAsync(Usuario user);
        Task ActualizarSeguridadUsuarioAsync(UsuarioSeguridad seguridad);
        Task<UsuarioSeguridad> GetSeguridadUsuario(int idUsuario);
        Task<Usuario?> GetUserById(int id);
    }
}
