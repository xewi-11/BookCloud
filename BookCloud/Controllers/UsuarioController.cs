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
        public UsuarioController(RepositoryUsuarios repo, FotoUsuario helper)
        {
            this._repo = repo;
            this._fotoHelper = helper;
        }
        public async Task<IActionResult> IndexPerfil()
        {
            var idUsuario = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(idUsuario))
                return RedirectToAction("Index", "Auth");

            Usuario user = await _repo.GetInfoUsario(idUsuario);
            if (user.Foto != null)
            {
                string pathweb = this._fotoHelper.MapUrlPath(user.Foto, Folder.Usuarios, user.Id);

                ViewData["PATHWEB"] = pathweb;
            }
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

                    byte[] passwordHash = Encryption.EncryptPassword(Contraseña, usuarioExistente.Salt);
                    usuarioExistente.PassWordHash = passwordHash;
                }

                // Procesar foto si se subió una nueva
                if (Foto != null && Foto.Length > 0)
                {
                    string fileName = Foto.FileName;
                    string fileNameWithId = $"{usuarioExistente.Id}{fileName}";
                    usuarioExistente.Foto = fileNameWithId;

                    string path = this._fotoHelper.MapPath(fileName, Folder.Usuarios, usuarioExistente.Id);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        await Foto.CopyToAsync(stream);
                    }
                    string pathweb = this._fotoHelper.MapUrlPath(fileName, Folder.Usuarios, usuarioExistente.Id);
                    ViewData["PATHWEB"] = pathweb;

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