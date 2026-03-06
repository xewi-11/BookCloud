using BookCloud.Models;

namespace BookCloud.Repositories.Interfaces
{
    public interface IRepositoryFavoritos
    {
        Task<List<Favorito>> GetFavoritosByUsuarioId(int usuarioId);
        Task<Favorito> GetFavoritoByUsuarioAndLibro(int usuarioId, int libroId);
        Task InsertFavorito(Favorito favorito);
        Task DeleteFavorito(int id);

        // ✅ Nuevos métodos para el controlador
        Task<List<Libro>> GetFavoritosByUsuario(int usuarioId);
        Task AddFavorito(int usuarioId, int libroId);
        Task RemoveFavorito(int usuarioId, int libroId);
    }
}