using BookCloud.Helpers;
using BookCloud.Models;
using BookCloud.Models.ViewModels;
using BookCloud.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCloud.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IRepositoryPedidos _repoPedidos;
        private readonly IRepositoryWallet _repoWallet;
        private readonly IRepositoryLibros _repoLibros;
        private readonly IRepositoryPagos _repoPagos;

        public PedidoController(
            IRepositoryPedidos repoPedidos,
            IRepositoryWallet repoWallet,
            IRepositoryLibros repoLibros,
            IRepositoryPagos repoPagos)
        {
            _repoPedidos = repoPedidos;
            _repoWallet = repoWallet;
            _repoLibros = repoLibros;
            _repoPagos = repoPagos;
        }

        [HttpGet]
        public async Task<IActionResult> SeleccionarMetodoPago()
        {
            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            var carrito = CarritoHelper.GetCarrito(HttpContext.Session);
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            var total = carrito.Sum(i => i.Subtotal);
            var saldo = await _repoWallet.GetSaldoUsuario(int.Parse(usuarioId));

            var model = new MetodoPagoViewModel
            {
                Total = total,
                SaldoDisponible = saldo,
                TieneSaldoSuficiente = saldo >= total
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult SeleccionarMetodoPago(string metodoPago)
        {
            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            var carrito = CarritoHelper.GetCarrito(HttpContext.Session);
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            var total = carrito.Sum(i => i.Subtotal);

            // Si elige Wallet, procesar directamente
            if (metodoPago == "Wallet")
            {
                return RedirectToAction("ProcesarPago", new { metodoPago = "Wallet" });
            }

            // Si elige Tarjeta, ir a capturar datos
            if (metodoPago == "Tarjeta")
            {
                var model = new DatosTarjetaViewModel
                {
                    Total = total
                };
                return View("DatosTarjeta", model);
            }

            TempData["Error"] = "Método de pago no válido";
            return RedirectToAction("SeleccionarMetodoPago");
        }

        [HttpGet]
        public IActionResult DatosTarjeta()
        {
            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            var carrito = CarritoHelper.GetCarrito(HttpContext.Session);
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            var total = carrito.Sum(i => i.Subtotal);
            var model = new DatosTarjetaViewModel
            {
                Total = total
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarPagoTarjeta(DatosTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ✅ Limpiar datos sensibles antes de devolver la vista
                model.NumeroTarjeta = "";
                model.CVV = "";
                model.FechaVencimiento = "";
                model.NombreTitular = "";
                return View("DatosTarjeta", model);
            }

            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            var carrito = CarritoHelper.GetCarrito(HttpContext.Session);
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            try
            {
                var userId = int.Parse(usuarioId);
                var total = carrito.Sum(i => i.Subtotal);

                // Simulación de validación de tarjeta (limpiar espacios para validar)
                var numeroTarjetaLimpio = model.NumeroTarjeta.Replace(" ", "");
                model.NumeroTarjeta = numeroTarjetaLimpio; // Guardar sin espacios

                if (!ValidarTarjeta(model))
                {
                    ModelState.AddModelError("", "Error al procesar el pago. Verifique los datos de la tarjeta.");
                    // ✅ Limpiar datos sensibles
                    model.NumeroTarjeta = "";
                    model.CVV = "";
                    model.FechaVencimiento = "";
                    model.NombreTitular = "";
                    return View("DatosTarjeta", model);
                }

                // Crear detalles del pedido
                var detalles = carrito.Select(item => new PedidoDetalle
                {
                    LibroId = item.LibroId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio,
                    Activo = true
                }).ToList();

                // Crear el pedido
                var pedidoId = await _repoPedidos.CrearPedido(userId, total, detalles);

                // ✅ GUARDAR registro de pago en la base de datos
                var pago = new Pago
                {
                    PedidoId = pedidoId,
                    FechaPago = DateTime.Now,
                    Monto = total,
                    Metodo = "Tarjeta",
                    Estado = "Completado",
                    Activo = true
                };
                await _repoPagos.CrearPago(pago);

                // ✅ Transferir saldo a los vendedores
                await _repoWallet.TransferirSaldoAVendedores(pedidoId, userId);

                // Actualizar estado del pedido
                await _repoPedidos.ActualizarEstadoPedido(pedidoId, "Completado");

                // Limpiar el carrito
                CarritoHelper.LimpiarCarrito(HttpContext.Session);

                TempData["Mensaje"] = "¡Compra realizada exitosamente con tarjeta!";
                return RedirectToAction("ConfirmacionPedido", new { id = pedidoId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al procesar el pago: {ex.Message}");
                return View("DatosTarjeta", model);
            }
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> ProcesarPago(string metodoPago)
        {
            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            var carrito = CarritoHelper.GetCarrito(HttpContext.Session);
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            try
            {
                var userId = int.Parse(usuarioId);
                var total = carrito.Sum(i => i.Subtotal);

                // Validar método de pago Wallet
                if (metodoPago == "Wallet")
                {
                    var saldo = await _repoWallet.GetSaldoUsuario(userId);
                    if (saldo < total)
                    {
                        TempData["Error"] = "Saldo insuficiente en la Wallet";
                        return RedirectToAction("SeleccionarMetodoPago");
                    }

                    // Crear detalles del pedido
                    var detalles = carrito.Select(item => new PedidoDetalle
                    {
                        LibroId = item.LibroId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio,
                        Activo = true
                    }).ToList();

                    // Crear el pedido
                    var pedidoId = await _repoPedidos.CrearPedido(userId, total, detalles);

                    // Descontar del wallet (esto crea el registro en SaldoMovimientos)
                    await _repoWallet.DescontarSaldo(
                        userId,
                        pedidoId,
                        total,
                        "Compra de libros"
                    );

                    //  GUARDAR registro de pago en la base de datos
                    var pago = new Pago
                    {
                        PedidoId = pedidoId,
                        FechaPago = DateTime.Now,
                        Monto = total,
                        Metodo = "Wallet",
                        Estado = "Completado",
                        Activo = true
                    };
                    await _repoPagos.CrearPago(pago);

                    // ✅ Transferir saldo a los vendedores
                    await _repoWallet.TransferirSaldoAVendedores(pedidoId, userId);

                    // Actualizar estado del pedido
                    await _repoPedidos.ActualizarEstadoPedido(pedidoId, "Completado");

                    // Limpiar el carrito
                    CarritoHelper.LimpiarCarrito(HttpContext.Session);

                    TempData["Mensaje"] = "¡Compra realizada exitosamente con Wallet!";
                    return RedirectToAction("ConfirmacionPedido", new { id = pedidoId });
                }

                TempData["Error"] = "Método de pago no válido";
                return RedirectToAction("SeleccionarMetodoPago");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar el pago: {ex.Message}";
                return RedirectToAction("SeleccionarMetodoPago");
            }
        }

        private bool ValidarTarjeta(DatosTarjetaViewModel model)
        {
            // Simulación de validación de tarjeta
            return ValidarNumeroTarjetaLuhn(model.NumeroTarjeta);
        }

        private bool ValidarNumeroTarjetaLuhn(string numero)
        {
            // ✅ Limpiar espacios antes de validar
            numero = numero?.Replace(" ", "") ?? "";

            // Validar que solo contiene dígitos
            if (string.IsNullOrEmpty(numero) || !numero.All(char.IsDigit))
                return false;

            // Validar longitud
            if (numero.Length < 13 || numero.Length > 19)
                return false;

            // Algoritmo de Luhn para validar números de tarjeta
            int suma = 0;
            bool alternar = false;

            for (int i = numero.Length - 1; i >= 0; i--)
            {
                int digito = int.Parse(numero[i].ToString());

                if (alternar)
                {
                    digito *= 2;
                    if (digito > 9)
                        digito -= 9;
                }

                suma += digito;
                alternar = !alternar;
            }

            return (suma % 10 == 0);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmacionPedido(int id)
        {
            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            var pedido = await _repoPedidos.GetPedido(id);
            if (pedido == null || pedido.UsuarioId != int.Parse(usuarioId))
            {
                return NotFound();
            }

            return View(pedido);
        }

        [HttpGet]
        public async Task<IActionResult> MisPedidos()
        {
            var usuarioId = HttpContext.Session.GetString("Id");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            // You likely want to get the user's orders and return a view.
            var pedidos = await _repoPedidos.GetPedidosUsuario(int.Parse(usuarioId));
            return View(pedidos);
        }
    }
}

