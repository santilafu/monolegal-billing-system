using Microsoft.AspNetCore.Mvc;
using Monolegal.API.Models;
using Monolegal.API.Services;

namespace Monolegal.API.Controllers
{
    /// <summary>
    /// Controlador principal de la API para gestionar facturas.
    ///
    /// ARQUITECTURA REST: Cada acción HTTP tiene un verbo y una URL clara:
    ///   GET  /api/facturas          → Obtener todas las facturas (para Angular)
    ///   POST /api/facturas/procesar → Ejecutar el proceso de recordatorios
    ///
    /// [ApiController] activa validaciones automáticas y respuestas estándar.
    /// [Route] define la URL base del controlador.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FacturasController : ControllerBase
    {
        private readonly FacturaService _facturaService;
        private readonly IEmailService _emailService;
        private readonly ILogger<FacturasController> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// ASP.NET Core pasa automáticamente los servicios registrados en Program.cs.
        /// </summary>
        public FacturasController(
            FacturaService facturaService,
            IEmailService emailService,
            ILogger<FacturasController> logger)
        {
            _facturaService = facturaService;
            _emailService = emailService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/facturas
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Devuelve todas las facturas de la base de datos.
        /// Este endpoint lo consume Angular para mostrar el resumen en pantalla.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Factura>>> Get()
        {
            var facturas = await _facturaService.GetAsync();
            return Ok(facturas);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/facturas/procesar
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta el proceso de recordatorios para todas las facturas pendientes.
        ///
        /// LÓGICA DEL PROCESO (por cada factura pendiente):
        ///   1. Si estado == "primerrecordatorio"
        ///      → Enviar email: "Ha pasado a segundo recordatorio"
        ///      → Actualizar estado a "segundorecordatorio"
        ///
        ///   2. Si estado == "segundorecordatorio"
        ///      → Enviar email: "Su servicio va a ser desactivado"
        ///      → Actualizar estado a "desactivado"
        ///
        /// Solo se procesan facturas en esos dos estados — las "desactivadas"
        /// o en otros estados se ignoran automáticamente.
        /// </summary>
        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarFacturas()
        {
            // Obtenemos solo las facturas que necesitan procesarse (filtro en BD)
            var pendientes = await _facturaService.GetPendientesAsync();

            if (!pendientes.Any())
            {
                return Ok(new { mensaje = "No hay facturas pendientes de procesar." });
            }

            int procesadas = 0;
            var errores = new List<string>();

            foreach (var factura in pendientes)
            {
                try
                {
                    if (factura.Estado == "primerrecordatorio")
                    {
                        await ProcesarPrimerRecordatorio(factura);
                    }
                    else if (factura.Estado == "segundorecordatorio")
                    {
                        await ProcesarSegundoRecordatorio(factura);
                    }

                    procesadas++;
                }
                catch (Exception ex)
                {
                    // Si una factura falla, registramos el error y continuamos con las demás.
                    // No queremos que un fallo individual detenga todo el proceso.
                    var errorMsg = $"Error en factura {factura.Id} ({factura.NombreCliente}): {ex.Message}";
                    errores.Add(errorMsg);
                    _logger.LogError(ex, "Error al procesar factura {FacturaId}", factura.Id);
                }
            }

            return Ok(new
            {
                mensaje = "Proceso de recordatorios completado.",
                totalPendientes = pendientes.Count,
                procesadas,
                fallidas = errores.Count,
                errores
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÉTODOS PRIVADOS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Procesa una factura en "primerrecordatorio":
        /// envía email de segundo aviso y actualiza el estado.
        /// </summary>
        private async Task ProcesarPrimerRecordatorio(Factura factura)
        {
            var asunto = "Segundo recordatorio de pago - Monolegal";
            var cuerpo = GenerarEmailSegundoRecordatorio(factura);

            // Primero enviamos el email; si falla aquí, el estado NO se actualiza
            await _emailService.SendEmailAsync(factura.EmailCliente, asunto, cuerpo);

            // Solo actualizamos el estado después de confirmar que el email se envió
            await _facturaService.UpdateEstadoAsync(factura.Id!, "segundorecordatorio");

            _logger.LogInformation(
                "Factura {Id} ({Cliente}) procesada: primer → segundo recordatorio",
                factura.Id, factura.NombreCliente);
        }

        /// <summary>
        /// Procesa una factura en "segundorecordatorio":
        /// envía email de desactivación y actualiza el estado.
        /// </summary>
        private async Task ProcesarSegundoRecordatorio(Factura factura)
        {
            var asunto = "Aviso de desactivación de servicio - Monolegal";
            var cuerpo = GenerarEmailDesactivacion(factura);

            await _emailService.SendEmailAsync(factura.EmailCliente, asunto, cuerpo);
            await _facturaService.UpdateEstadoAsync(factura.Id!, "desactivado");

            _logger.LogInformation(
                "Factura {Id} ({Cliente}) procesada: segundo recordatorio → desactivado",
                factura.Id, factura.NombreCliente);
        }

        /// <summary>
        /// Plantilla HTML del email para el segundo recordatorio.
        /// </summary>
        private static string GenerarEmailSegundoRecordatorio(Factura factura) => $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333; padding: 20px;">
                <h2 style="color: #e67e22;">Segundo Recordatorio de Pago</h2>
                <p>Estimado/a <strong>{factura.NombreCliente}</strong>,</p>
                <p>
                    Le informamos que su factura por importe de <strong>{factura.Monto:C}</strong>
                    sigue pendiente de pago y ha pasado a <strong>segundo recordatorio</strong>.
                </p>
                <p>
                    Si no regulariza su situación en los próximos días, su servicio
                    podría ser <strong>suspendido</strong>.
                </p>
                <p>Por favor, realice el pago a la mayor brevedad posible.</p>
                <hr/>
                <p style="font-size: 12px; color: #999;">Mensaje automático — Monolegal Billing System</p>
            </body>
            </html>
            """;

        /// <summary>
        /// Plantilla HTML del email de aviso de desactivación.
        /// </summary>
        private static string GenerarEmailDesactivacion(Factura factura) => $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333; padding: 20px;">
                <h2 style="color: #e74c3c;">Aviso de Desactivación de Servicio</h2>
                <p>Estimado/a <strong>{factura.NombreCliente}</strong>,</p>
                <p>
                    Lamentamos informarle que, dado que la factura de
                    <strong>{factura.Monto:C}</strong> continúa sin ser abonada,
                    su servicio ha sido <strong>desactivado</strong>.
                </p>
                <p>Para reactivar el servicio, contacte con nosotros y regularice la deuda pendiente.</p>
                <hr/>
                <p style="font-size: 12px; color: #999;">Mensaje automático — Monolegal Billing System</p>
            </body>
            </html>
            """;
    }
}
