using BookCloud.Models;
using Microsoft.EntityFrameworkCore;

namespace BookCloud.Data
{
    public class BookCloudContext : DbContext
    {
        public BookCloudContext(DbContextOptions<BookCloudContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
