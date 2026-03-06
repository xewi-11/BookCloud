using BookCloud.Models;
using BookCloud.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BookCloud.Controllers
{
    public class FavoritoController : Controller
    {
        private IRepositoryLibros _repositoryLibros;
        private IRepositoryFavoritos _repositoryFavoritos;
        private IMemoryCache _memoryCache;

        public FavoritoController(
            IRepositoryLibros repositoryLibros,
            IRepositoryFavoritos repositoryFavoritos,
            IMemoryCache memoryCache)
        {
            this._repositoryLibros = repositoryLibros;
            this._repositoryFavoritos = repositoryFavoritos;
            this._memoryCache = memoryCache;
        }

        // ✅ Método auxiliar para obtener el usuario de la sesión
        private int ObtenerUsuarioIdActual()
        {
            var usuarioIdString = HttpContext.Session.GetString("Id");
            if (int.TryParse(usuarioIdString, out int usuarioId))
            {
                return usuarioId;
            }
            return 0;
        }

        // Ver todos los favoritos del usuario
        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioIdActual();

            if (usuarioId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<Libro> librosFavoritos;
            string cacheKey = $"FAVORITOS_{usuarioId}";

            // Verificar si existe en caché
            if (_memoryCache.TryGetValue(cacheKey, out librosFavoritos))
            {
                return View(librosFavoritos);
            }

            // Si no existe en caché, obtener de BD
            librosFavoritos = await _repositoryFavoritos.GetFavoritosByUsuario(usuarioId);

            // Guardar en caché por 10 minutos
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _memoryCache.Set(cacheKey, librosFavoritos, cacheOptions);

            return View(librosFavoritos);
        }

        // Agregar libro a favoritos desde cualquier vista
        public async Task<IActionResult> Agregar(int libroId)
        {
            var usuarioId = ObtenerUsuarioIdActual();

            if (usuarioId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<Libro> librosFavoritos;
            string cacheKey = $"FAVORITOS_{usuarioId}";

            // Verificar si ya existe en caché
            if (!_memoryCache.TryGetValue(cacheKey, out librosFavoritos))
            {
                librosFavoritos = new List<Libro>();
            }

            // Verificar que el libro no esté ya en favoritos
            if (!librosFavoritos.Any(l => l.Id == libroId))
            {
                var libro = await _repositoryLibros.GetLibro(libroId);

                if (libro == null)
                {
                    TempData["Error"] = "El libro no existe.";
                    return RedirectToAction("Index", "Libro");
                }

                // Agregar a BD
                await _repositoryFavoritos.AddFavorito(usuarioId, libroId);

                // Agregar a caché
                librosFavoritos.Add(libro);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(cacheKey, librosFavoritos, cacheOptions);

                TempData["Success"] = "Libro agregado a favoritos.";
            }
            else
            {
                TempData["Info"] = "El libro ya está en tus favoritos.";
            }

            // Redirigir de vuelta a la página anterior
            string returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Libro");
        }

        // Eliminar libro de favoritos
        public async Task<IActionResult> Eliminar(int libroId)
        {
            var usuarioId = ObtenerUsuarioIdActual();

            if (usuarioId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Eliminar de BD
            await _repositoryFavoritos.RemoveFavorito(usuarioId, libroId);

            // Actualizar caché
            string cacheKey = $"FAVORITOS_{usuarioId}";
            if (_memoryCache.TryGetValue(cacheKey, out List<Libro> librosFavoritos))
            {
                var libroAEliminar = librosFavoritos.FirstOrDefault(l => l.Id == libroId);
                if (libroAEliminar != null)
                {
                    librosFavoritos.Remove(libroAEliminar);

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                    _memoryCache.Set(cacheKey, librosFavoritos, cacheOptions);
                }
            }

            TempData["Success"] = "Libro eliminado de favoritos.";
            return RedirectToAction("Index");
        }

        // Verificar si un libro está en favoritos (usado en vistas)
        public async Task<IActionResult> EstaEnFavoritos(int libroId)
        {
            var usuarioId = ObtenerUsuarioIdActual();

            if (usuarioId == 0)
            {
                return Json(new { esFavorito = false });
            }

            string cacheKey = $"FAVORITOS_{usuarioId}";
            List<Libro> librosFavoritos;

            if (!_memoryCache.TryGetValue(cacheKey, out librosFavoritos))
            {
                librosFavoritos = await _repositoryFavoritos.GetFavoritosByUsuario(usuarioId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(cacheKey, librosFavoritos, cacheOptions);
            }

            var esFavorito = librosFavoritos.Any(l => l.Id == libroId);
            return Json(new { esFavorito });
        }
    }
}