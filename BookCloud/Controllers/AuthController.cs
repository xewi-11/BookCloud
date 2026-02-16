using BookCloud.Helpers;
using BookCloud.Models;
using BookCloud.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookCloud.Controllers
{
    public class AuthController : Controller
    {
        private RepositoryUsuarios _repo;
        public AuthController(RepositoryUsuarios repo)
        {
            this._repo = repo;
        }
        [HttpGet]
        public IActionResult RegisterUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUSer usuario)
        {
            if (usuario == null)
            {

                return View();

            }

            string salt = Encryption.GenerateSalt();
            byte[] encryptedPassword = Encryption.EncryptPassword(usuario.Pass, salt);

            Usuario user = new Usuario
            {

                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                PassWordHash = encryptedPassword,
                Salt = salt,
                FechaRegistro = DateTime.Now,
                Activo = true,
                Foto = null
            };

            await this._repo.CreateUserASync(user);
            return RedirectToAction("Login");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Credenciales cred)
        {
            Usuario s = await this._repo.GetUserByEmail(cred.Correo);
            if (s != null)
            {
                // Verificar la contraseña
                byte[] hash = Encryption.EncryptPassword(cred.Pass, s.Salt);
                if (Encryption.CompareArrays(hash, s.PassWordHash))
                {
                    // Autenticación exitosa
                    // Aquí puedes establecer la sesión o el token de autenticación
                    return RedirectToAction("Index", "Home");
                }
            }
            // Autenticación fallida
            ViewData["Error"] = "Correo o contraseña incorrectos.";
            return View();
        }
    }
}
//Usuario s = await this._repo.GetUserByEmail(usuario.Correo);