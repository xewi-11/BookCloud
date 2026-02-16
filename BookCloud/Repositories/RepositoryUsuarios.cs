using BookCloud.Data;
using BookCloud.Models;
using Microsoft.EntityFrameworkCore;

namespace BookCloud.Repositories
{
    public class RepositoryUsuarios
    {
        private BookCloudContext _context;

        public RepositoryUsuarios(BookCloudContext context)
        {
            this._context = context;
        }
        public async Task CreateUserASync(Usuario user)
        {
            var consulta = from datos in this._context.Usuarios select datos;

            int id = 1;
            user.Id = id;
            await this._context.Usuarios.AddAsync(user);
            await this._context.SaveChangesAsync();
        }

        public async Task<Usuario> GetUserByEmail(string email)
        {

            var consulta = from datos in this._context.Usuarios
                           where datos.Correo == email
                           select datos;

            Usuario user = await consulta.FirstOrDefaultAsync();

            if (user != null)
            {
                return user;
            }
            return null;
        }
    }
}
