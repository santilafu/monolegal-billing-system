namespace Monolegal.API.Services
{
    /// <summary>
    /// Contrato (interfaz) que define qué debe poder hacer cualquier servicio de email.
    ///
    /// PRINCIPIO DE INVERSIÓN DE DEPENDENCIAS (DIP): Los controladores dependen de
    /// esta interfaz, no de la clase concreta EmailService. Esto significa que si
    /// mañana quieres cambiar de Mailtrap a SendGrid, solo cambias EmailService —
    /// el controlador no necesita tocarse.
    ///
    /// TESTABILIDAD: En los tests unitarios puedes crear un "FakeEmailService" que
    /// implemente esta interfaz y no envíe emails de verdad.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email al destinatario indicado.
        /// </summary>
        /// <param name="destinatario">Dirección de email del destinatario.</param>
        /// <param name="asunto">Asunto del correo.</param>
        /// <param name="cuerpoHtml">Contenido del email en HTML.</param>
        Task SendEmailAsync(string destinatario, string asunto, string cuerpoHtml);
    }
}
