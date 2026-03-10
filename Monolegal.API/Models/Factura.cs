using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monolegal.API.Models
{
    /// <summary>
    /// Modelo que representa una factura en MongoDB.
    ///
    /// MAPEO OBJETO-DOCUMENTO: Esta clase C# se mapea directamente a un documento
    /// JSON en MongoDB. Cada propiedad es un campo del documento.
    ///
    /// Los atributos [BsonElement] permiten que el nombre en C# sea diferente
    /// al nombre en la base de datos (ej: "ClienteId" en C# = "cliente_id" en MongoDB).
    /// </summary>
    public class Factura
    {
        /// <summary>
        /// Identificador único del documento en MongoDB.
        /// MongoDB genera este ID automáticamente como un ObjectId de 24 caracteres.
        /// [BsonId] le dice al driver que este campo es la clave primaria.
        /// [BsonRepresentation] permite manejar el ObjectId como string en C#.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// ID del cliente al que pertenece la factura.
        /// Permite agrupar todas las facturas de un mismo cliente.
        /// </summary>
        [BsonElement("cliente_id")]
        public string ClienteId { get; set; } = null!;

        /// <summary>Nombre completo del cliente (ej: "Empresa ABC S.L.").</summary>
        public string NombreCliente { get; set; } = null!;

        /// <summary>
        /// Email del cliente. Se usa para enviarle los recordatorios.
        /// IMPORTANTE: Debe ser un email válido para que el envío funcione.
        /// </summary>
        public string EmailCliente { get; set; } = null!;

        /// <summary>Importe de la factura en la moneda configurada.</summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Estado actual de la factura en el proceso de cobro:
        /// - "primerrecordatorio"  → Se enviará aviso de segundo recordatorio
        /// - "segundorecordatorio" → Se enviará aviso de desactivación
        /// - "desactivado"         → Proceso completado, no se procesa más
        /// </summary>
        public string Estado { get; set; } = null!;

        /// <summary>Fecha en que se emitió la factura.</summary>
        public DateTime FechaFactura { get; set; }
    }
}
