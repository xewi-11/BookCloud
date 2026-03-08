using BookCloud.Data;
using BookCloud.Models;
using BookCloud.Models.ViewModels;
using BookCloud.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BookCloud.Controllers
{
    public class WalletController : Controller
    {
        private readonly IRepositoryWallet _repoWallet;
        private readonly BookCloudContext _context;

        public WalletController(IRepositoryWallet repoWallet, BookCloudContext context)
        {
            _repoWallet = repoWallet;
            _context = context;
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

            var viewModel = new WalletViewModel
            {
                SaldoActual = await _repoWallet.GetSaldoUsuario(usuarioId),
                Movimientos = await _repoWallet.GetMovimientos(usuarioId, 20)
            };

            // Calcular totales
            viewModel.TotalIngresos = viewModel.Movimientos
                .Where(m => m.Tipo == "Ingreso")
                .Sum(m => m.Monto);

            viewModel.TotalGastos = viewModel.Movimientos
                .Where(m => m.Tipo == "Pago" || m.Tipo == "Retiro")
                .Sum(m => m.Monto);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Movimientos(int limite = 50, string filtro = "Todos")
        {
            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == 0)
                return RedirectToAction("Login", "Auth");

            // Obtener movimientos de Wallet
            var movimientosWallet = await _context.SaldoMovimientos
                .Where(m => m.UsuarioId == usuarioId && m.Activo)
                .Select(m => new MovimientoUnificadoViewModel
                {
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Metodo = "Wallet",
                    Monto = m.Monto,
                    Descripcion = m.Descripcion,
                    PedidoId = m.PedidoId
                })
                .ToListAsync();

            // Obtener pagos con tarjeta
            var pagosTarjeta = await _context.Pagos
                .Include(p => p.Pedido)
                .Where(p => p.Pedido.UsuarioId == usuarioId && p.Activo && p.Metodo == "Tarjeta")
                .Select(p => new MovimientoUnificadoViewModel
                {
                    Fecha = p.FechaPago,
                    Tipo = "Pago",
                    Metodo = "Tarjeta",
                    Monto = p.Monto,
                    Descripcion = $"Compra con tarjeta - Pedido #{p.PedidoId}",
                    PedidoId = p.PedidoId
                })
                .ToListAsync();

            // Combinar todos los movimientos
            var todosMovimientos = movimientosWallet
                .Concat(pagosTarjeta)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            // Aplicar filtro según parámetro
            var movimientosFiltrados = filtro switch
            {
                "Wallet" => todosMovimientos.Where(m => m.Metodo == "Wallet").Take(limite).ToList(),
                "Tarjeta" => todosMovimientos.Where(m => m.Metodo == "Tarjeta").Take(limite).ToList(),
                _ => todosMovimientos.Take(limite).ToList() // "Todos"
            };

            var viewModel = new MovimientosViewModel
            {
                SaldoActual = await _repoWallet.GetSaldoUsuario(usuarioId),
                Movimientos = movimientosFiltrados
            };

            // Calcular totales sobre todos los movimientos (no filtrados)
            viewModel.TotalIngresos = todosMovimientos
                .Where(m => m.Tipo == "Ingreso")
                .Sum(m => m.Monto);

            viewModel.TotalGastos = todosMovimientos
                .Where(m => m.Tipo == "Pago" || m.Tipo == "Retiro")
                .Sum(m => m.Monto);

            viewModel.TotalGastosWallet = todosMovimientos
                .Where(m => (m.Tipo == "Pago" || m.Tipo == "Retiro") && m.Metodo == "Wallet")
                .Sum(m => m.Monto);

            viewModel.TotalGastosTarjeta = todosMovimientos
                .Where(m => m.Tipo == "Pago" && m.Metodo == "Tarjeta")
                .Sum(m => m.Monto);

            ViewBag.FiltroActual = filtro;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RecargarSaldo(RecargaTarjetaViewModel model)
        {
            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == 0)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor, completa correctamente todos los campos de la tarjeta.";
                return RedirectToAction("Index");
            }

            try
            {
                // Validar tarjeta (algoritmo de Luhn)
                if (!ValidarNumeroTarjeta(model.NumeroTarjeta.Replace(" ", "")))
                {
                    TempData["Error"] = "El número de tarjeta no es válido.";
                    return RedirectToAction("Index");
                }

                // Validar fecha de vencimiento
                if (!ValidarFechaVencimiento(model.FechaVencimiento))
                {
                    TempData["Error"] = "La tarjeta ha vencido o la fecha no es válida.";
                    return RedirectToAction("Index");
                }

                // Simular procesamiento de pago (en producción, aquí integrarías con Stripe, PayPal, etc.)
                await Task.Delay(1000); // Simulando latencia de API de pago

                // Registrar recarga en la wallet
                var ultimosDigitos = model.NumeroTarjeta.Replace(" ", "").Substring(model.NumeroTarjeta.Replace(" ", "").Length - 4);
                var descripcion = $"Recarga con tarjeta ****{ultimosDigitos}";

                await _repoWallet.RecargarSaldo(usuarioId, model.Monto, descripcion);

                TempData["Mensaje"] = $"¡Recarga exitosa! Se agregaron ${model.Monto:N2} a tu wallet.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar el pago: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // Algoritmo de Luhn para validar números de tarjeta
        private bool ValidarNumeroTarjeta(string numero)
        {
            if (string.IsNullOrEmpty(numero) || !Regex.IsMatch(numero, @"^\d+$"))
                return false;

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

            return (suma % 10) == 0;
        }

        // Validar que la tarjeta no haya vencido
        private bool ValidarFechaVencimiento(string fecha)
        {
            if (string.IsNullOrEmpty(fecha) || !Regex.IsMatch(fecha, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                return false;

            var partes = fecha.Split('/');
            int mes = int.Parse(partes[0]);
            int anio = 2000 + int.Parse(partes[1]);

            var fechaVencimiento = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));
            return fechaVencimiento >= DateTime.Now;
        }

    }
}