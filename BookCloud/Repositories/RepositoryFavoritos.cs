using BookCloud.Data;
using BookCloud.Models;
using BookCloud.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCloud.Repositories
{
    public class RepositoryFavoritos : IRepositoryFavoritos
    {
        private BookCloudContext _context;

        public RepositoryFavoritos(BookCloudContext context)
        {
            this._context = context;
        }

        public async Task<List<Favorito>> GetFavoritosByUsuarioId(int usuarioId)
        {
            return await _context.Favoritos
                .Include(f => f.Libro)
                .Where(f => f.UsuarioId == usuarioId && f.Activo)
                .ToListAsync();
        }

        public async Task<Favorito> GetFavoritoByUsuarioAndLibro(int usuarioId, int libroId)
        {
            return await _context.Favoritos
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.LibroId == libroId && f.Activo);
        }

        public async Task InsertFavorito(Favorito favorito)
        {
            await _context.Favoritos.AddAsync(favorito);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFavorito(int id)
        {
            var favorito = await _context.Favoritos.FindAsync(id);
            if (favorito != null)
            {
                favorito.Activo = false;
                _context.Favoritos.Update(favorito);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Nuevos métodos implementados
        public async Task<List<Libro>> GetFavoritosByUsuario(int usuarioId)
        {
            return await _context.Favoritos
                .Include(f => f.Libro)
                .Where(f => f.UsuarioId == usuarioId && f.Activo)
                .Select(f => f.Libro)
                .ToListAsync();
        }

        public async Task AddFavorito(int usuarioId, int libroId)
        {
            // Verificar si ya existe
            var existente = await GetFavoritoByUsuarioAndLibro(usuarioId, libroId);

            if (existente != null)
            {
                // Si existe pero está inactivo, reactivarlo
                if (!existente.Activo)
                {
                    existente.Activo = true;
                    existente.FechaAgregado = DateTime.Now;
                    _context.Favoritos.Update(existente);
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // Si no existe, crear uno nuevo
            var favorito = new Favorito
            {
                UsuarioId = usuarioId,
                LibroId = libroId,
                FechaAgregado = DateTime.Now,
                Activo = true
            };

            await InsertFavorito(favorito);
        }

        public async Task RemoveFavorito(int usuarioId, int libroId)
        {
            var favorito = await GetFavoritoByUsuarioAndLibro(usuarioId, libroId);

            if (favorito != null)
            {
                await DeleteFavorito(favorito.Id);
            }
        }
    }
}