using BookCloud.Models;

namespace BookCloud.Repositories.Interfaces
{
    public interface IRepositoryUsuarios
    {

        Task CreateUserASync(Usuario user);
        Task<Usuario> GetUserByEmail(string email);

        Task<Usuario> GetInfoUsario(string id);
        Task ActualizarUsuarioAsync(Usuario user);
    }
}
