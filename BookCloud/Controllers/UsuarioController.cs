using BookCloud.Helpers;
using BookCloud.Models;
using BookCloud.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookCloud.Controllers
{
    public class UsuarioController : Controller
    {
        private RepositoryUsuarios _repo;
        private readonly FotoUsuario _fotoHelper;
        public UsuarioController(RepositoryUsuarios repo)
        {
            this._repo = repo;
            this._fotoHelper = new FotoUsuario();
        }
        public async Task<IActionResult> IndexPerfil()
        {
            var idUsuario = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(idUsuario))
                return RedirectToAction("Index", "Auth");

            Usuario user = await _repo.GetInfoUsario(idUsuario);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(string Nombre, string Correo, string? Contraseña, IFormFile? Foto)
        {
            try
            {
                var idUsuario = HttpContext.Session.GetString("Id");
                if (string.IsNullOrEmpty(idUsuario))
                    return RedirectToAction("Index", "Auth");

                // Obtener usuario actual de la BD
                var usuarioExistente = await _repo.GetInfoUsario(idUsuario);
                if (usuarioExistente == null)
                    return NotFound();

                // Actualizar campos básicos
                usuarioExistente.Nombre = Nombre;
                usuarioExistente.Correo = Correo;

                // Actualizar contraseña solo si se proporcionó una nueva
                if (!string.IsNullOrWhiteSpace(Contraseña))
                {
                    // Generar nuevo salt y hashear la contraseña
                    string nuevoSalt = Encryption.GenerateSalt();
                    byte[] passwordHash = Encryption.EncryptPassword(Contraseña, nuevoSalt);

                    usuarioExistente.Salt = nuevoSalt;
                    usuarioExistente.PassWordHash = passwordHash;
                }

                // Procesar foto si se subió una nueva
                if (Foto != null && Foto.Length > 0)
                {
                    var resultado = await _fotoHelper.GuardarFotoAsync(Foto, idUsuario);
                    if (resultado.exito)
                    {
                        // Eliminar foto anterior si existe
                        if (!string.IsNullOrEmpty(usuarioExistente.Foto))
                            _fotoHelper.EliminarFoto(usuarioExistente.Foto);

                        usuarioExistente.Foto = resultado.rutaRelativa;
                    }
                    else
                    {
                        TempData["Error"] = resultado.error;
                        return View("IndexPerfil", usuarioExistente);
                    }
                }

                // Guardar cambios en BD
                await _repo.ActualizarUsuarioAsync(usuarioExistente);

                TempData["Mensaje"] = "Perfil actualizado correctamente";
                return RedirectToAction("IndexPerfil");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el perfil";
                return RedirectToAction("IndexPerfil");
            }
        }
    }
}