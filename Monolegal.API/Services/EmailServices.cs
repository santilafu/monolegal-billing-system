using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Monolegal.API.Models;

namespace Monolegal.API.Services
{
    /// <summary>
    /// Implementación del servicio de email usando MailKit.
    ///
    /// MailKit es la librería de email recomendada para .NET — es robusta,
    /// bien mantenida y soporta TLS/SSL correctamente.
    ///
    /// PARA PRUEBAS: Configura Mailtrap en appsettings.json.
    ///   Los emails "se envían" pero no llegan a nadie real — aparecen en
    ///   tu bandeja de Mailtrap. Perfecto para desarrollo y demos.
    ///
    /// PARA PRODUCCIÓN: Cambia las credenciales en appsettings.json por las
    ///   de tu proveedor real (Gmail, SendGrid, Mailgun, etc).
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        // ILogger permite registrar errores en los logs de la aplicación
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// Recibe la configuración del email desde appsettings.json.
        /// </summary>
        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Envía un email real usando el protocolo SMTP con MailKit.
        ///
        /// El proceso es:
        ///   1. Construir el mensaje (remitente, destinatario, asunto, cuerpo)
        ///   2. Abrir conexión segura con el servidor SMTP
        ///   3. Autenticarse con usuario y contraseña
        ///   4. Enviar el mensaje
        ///   5. Cerrar la conexión
        /// </summary>
        /// <param name="destinatario">Email del cliente (ej: cliente@empresa.com).</param>
        /// <param name="asunto">Asunto del email.</param>
        /// <param name="cuerpoHtml">Cuerpo del email en formato HTML.</param>
        public async Task SendEmailAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            try
            {
                // --- PASO 1: Construir el mensaje ---
                var mensaje = new MimeMessage();

                // Remitente (quien envía)
                mensaje.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));

                // Destinatario (el cliente)
                mensaje.To.Add(MailboxAddress.Parse(destinatario));

                mensaje.Subject = asunto;

                // El cuerpo puede ser HTML — esto permite emails con formato bonito
                mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

                // --- PASO 2: Conectar y enviar ---
                using var smtp = new SmtpClient();

                // StartTls: cifra la conexión para que nadie pueda interceptar las credenciales
                await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);

                // Autenticación con las credenciales del appsettings
                await smtp.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword);

                await smtp.SendAsync(mensaje);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email enviado a {Destinatario} con asunto '{Asunto}'", destinatario, asunto);
            }
            catch (Exception ex)
            {
                // Si el email falla, lo registramos en los logs pero NO lanzamos excepción
                // para que el proceso de actualización de estado pueda continuar
                _logger.LogError(ex, "Error al enviar email a {Destinatario}", destinatario);
                throw; // Re-lanzamos para que el controlador sepa que falló
            }
        }
    }
}
