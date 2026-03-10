namespace Monolegal.API.Models
{
    /// <summary>
    /// Clase de configuración para el servicio de envío de emails.
    ///
    /// Los valores se cargan desde la sección "EmailSettings" de appsettings.json.
    /// Para desarrollo se puede usar Mailtrap (https://mailtrap.io) que captura
    /// los correos sin enviarlos realmente — ideal para pruebas.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Servidor SMTP. Ejemplos:
        /// - Gmail:    smtp.gmail.com (puerto 587)
        /// - Mailtrap: sandbox.smtp.mailtrap.io (puerto 587, para pruebas)
        /// </summary>
        public string SmtpHost { get; set; } = null!;

        /// <summary>Puerto del servidor SMTP (normalmente 587 con TLS).</summary>
        public int SmtpPort { get; set; }

        /// <summary>Usuario del servidor SMTP.</summary>
        public string SmtpUser { get; set; } = null!;

        /// <summary>
        /// Contraseña del servidor SMTP.
        /// SEGURIDAD: Nunca pongas la contraseña real aquí en texto plano.
        /// Usa "dotnet user-secrets" para desarrollo local.
        /// </summary>
        public string SmtpPassword { get; set; } = null!;

        /// <summary>Dirección de email que aparece como remitente.</summary>
        public string FromAddress { get; set; } = null!;

        /// <summary>Nombre que aparece como remitente en el email.</summary>
        public string FromName { get; set; } = null!;
    }
}
