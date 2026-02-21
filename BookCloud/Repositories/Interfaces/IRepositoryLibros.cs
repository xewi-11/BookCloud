using BookCloud.Models;

namespace BookCloud.Repositories.Interfaces
{
    public interface IRepositoryLibros
    {
        Task<List<Libro>> GetLibros();
        Task<Libro> GetLibro(int id);
        Task InsertLibro(Libro libro);
        Task UpdateLibro(Libro libro);
        Task DeleteLibro(int id);
    }
}
