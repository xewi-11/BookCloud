using BookCloud.Helpers;
using BookCloud.Models;
using BookCloud.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCloud.Controllers
{
    public class AuthController : Controller
    {
        private IRepositoryUsuarios _repo;

        public AuthController(IRepositoryUsuarios repo)
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
                Password = usuario.Pass, // Contraseña en texto plano opcional
                FechaRegistro = DateTime.Now,
                Activo = true,
                Foto = null
            };

            UsuarioSeguridad seguridad = new UsuarioSeguridad
            {
                PasswordHash = encryptedPassword,
                Salt = salt
            };

            await this._repo.CreateUserASync(user, seguridad);
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
            if (s != null && s.UsuarioSeguridad != null)
            {
                // Verificar la contraseña
                byte[] hash = Encryption.EncryptPassword(cred.Password, s.UsuarioSeguridad.Salt);
                if (Encryption.CompareArrays(hash, s.UsuarioSeguridad.PasswordHash))
                {
                    // Autenticación exitosa
                    HttpContext.Session.SetString("Id", s.Id.ToString());
                    HttpContext.Session.SetString("Nombre", s.Nombre);
                    HttpContext.Session.SetString("Correo", s.Correo);
                    // ✅ Agregar foto a la sesión
                    if (!string.IsNullOrEmpty(s.Foto))
                    {
                        HttpContext.Session.SetString("FOTO", s.Foto);
                    }
                    return RedirectToAction("Index", "Libro");
                }
            }
            // Autenticación fallida
            ViewData["Error"] = "Correo o contraseña incorrectos.";
            return View();
        }


        public IActionResult Logout()
        {
            // Limpiar toda la sesión del usuario
            HttpContext.Session.Clear();

            // Redirigir a la página de login
            return RedirectToAction("Login");
        }
    }
}