using BookCloud.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCloud.Controllers
{
    public class ChatController : Controller
    {
        private readonly IRepositoryChats _repositoryChats;
        private readonly IRepositoryUsuarios _repositoryUsuarios;
        private readonly IRepositoryLibros _repositoryLibros;

        public ChatController(
            IRepositoryChats repositoryChats,
            IRepositoryUsuarios repositoryUsuarios,
            IRepositoryLibros repositoryLibros)
        {
            _repositoryChats = repositoryChats;
            _repositoryUsuarios = repositoryUsuarios;
            _repositoryLibros = repositoryLibros;
        }

        private int ObtenerUsuarioIdActual()
        {
            var usuarioIdString = HttpContext.Session.GetString("Id");
            if (int.TryParse(usuarioIdString, out int usuarioId))
            {
                return usuarioId;
            }
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == 0)
                return RedirectToAction("Login", "Auth");

            var chats = await _repositoryChats.ObtenerChatsDeUsuarioAsync(usuarioId);
            ViewBag.UsuarioActualId = usuarioId;
            return View(chats);
        }

        // ✅ Actualizado para recibir el ID del libro
        public async Task<IActionResult> Conversacion(int id, int? libroId = null)
        {
            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == 0)
                return RedirectToAction("Login", "Auth");

            var chat = await _repositoryChats.ObtenerChatPorIdAsync(id);
            if (chat == null)
                return NotFound();

            if (chat.Usuario1Id != usuarioId && chat.Usuario2Id != usuarioId)
                return Forbid();

            var mensajes = await _repositoryChats.ObtenerMensajesDelChatAsync(id);

            ViewBag.ChatId = id;
            ViewBag.UsuarioActualId = usuarioId;
            ViewBag.OtroUsuario = chat.Usuario1Id == usuarioId ? chat.Usuario2 : chat.Usuario1;

            // ✅ Obtener información del libro si se proporciona
            if (libroId.HasValue)
            {
                var libro = await _repositoryLibros.GetLibro(libroId.Value);
                ViewBag.Libro = libro;
            }

            return View(mensajes);
        }

        // ✅ Actualizado para recibir el ID del libro
        public async Task<IActionResult> IniciarChat(int usuarioId, int? libroId = null)
        {
            var usuarioActualId = ObtenerUsuarioIdActual();
            if (usuarioActualId == 0)
                return RedirectToAction("Login", "Auth");

            if (usuarioId == usuarioActualId)
            {
                TempData["Error"] = "No puedes iniciar un chat contigo mismo";
                return RedirectToAction("Index", "Libro");
            }

            var usuarioDestino = await _repositoryUsuarios.GetUserById(usuarioId);
            if (usuarioDestino == null || !usuarioDestino.Activo)
            {
                TempData["Error"] = "El usuario no existe o no está disponible";
                return RedirectToAction("Index", "Libro");
            }

            var chat = await _repositoryChats.ObtenerOCrearChatAsync(usuarioActualId, usuarioId);

            // ✅ Redirigir con el libroId si existe
            if (libroId.HasValue)
            {
                return RedirectToAction("Conversacion", new { id = chat.Id, libroId = libroId.Value });
            }

            return RedirectToAction("Conversacion", new { id = chat.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerMensajes(int chatId)
        {
            var usuarioId = ObtenerUsuarioIdActual();

            if (!await _repositoryChats.UsuarioPerteneceChatAsync(chatId, usuarioId))
                return Forbid();

            var mensajes = await _repositoryChats.ObtenerMensajesDelChatAsync(chatId);

            return Json(mensajes.Select(m => new
            {
                id = m.Id,
                remitenteId = m.RemitenteId,
                remitenteNombre = m.Remitente?.Nombre ?? "Usuario",
                contenido = m.Contenido,
                fechaEnvio = m.FechaEnvio.ToString("HH:mm")
            }));
        }
    }
}