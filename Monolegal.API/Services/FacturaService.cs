using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Monolegal.API.Models;

namespace Monolegal.API.Services
{
    /// <summary>
    /// Servicio que gestiona todas las operaciones de facturas contra MongoDB.
    ///
    /// PATRÓN REPOSITORIO: Este servicio actúa como intermediario entre el controlador
    /// y la base de datos. El controlador no sabe nada de MongoDB — solo llama a
    /// métodos de este servicio. Esto hace el código más limpio y más fácil de testear.
    ///
    /// SINGLETON: Se registra como Singleton en Program.cs porque la conexión a MongoDB
    /// es cara de crear y es segura compartirla entre peticiones.
    /// </summary>
    public class FacturaService
    {
        // IMongoCollection es el objeto que representa la colección "Facturas" en MongoDB.
        // Es el equivalente a una tabla en SQL.
        private readonly IMongoCollection<Factura> _facturasCollection;

        /// <summary>
        /// Constructor. ASP.NET Core inyecta automáticamente IOptions con los valores
        /// del appsettings.json gracias al registro en Program.cs.
        /// </summary>
        /// <param name="settings">Configuración de MongoDB (host, base de datos, colección).</param>
        public FacturaService(IOptions<MongoDbSettings> settings)
        {
            // Creamos el cliente de MongoDB con la cadena de conexión del appsettings
            var client = new MongoClient(settings.Value.ConnectionString);

            // Seleccionamos la base de datos (ej: "MonolegalDB")
            var database = client.GetDatabase(settings.Value.DatabaseName);

            // Seleccionamos la colección tipada — MongoDB mapeará automáticamente
            // los documentos JSON a objetos C# de tipo Factura
            _facturasCollection = database.GetCollection<Factura>(settings.Value.CollectionName);
        }

        /// <summary>
        /// Obtiene TODAS las facturas de la base de datos.
        /// Usado por el endpoint GET para mostrar el resumen en Angular.
        /// </summary>
        /// <returns>Lista completa de facturas.</returns>
        public async Task<List<Factura>> GetAsync() =>
            await _facturasCollection.Find(_ => true).ToListAsync();

        /// <summary>
        /// Obtiene solo las facturas que necesitan ser procesadas.
        /// Filtra las que están en "primerrecordatorio" o "segundorecordatorio".
        /// Así no cargamos facturas ya desactivadas ni en otros estados.
        /// </summary>
        /// <returns>Lista de facturas pendientes de procesar.</returns>
        public async Task<List<Factura>> GetPendientesAsync()
        {
            // Creamos un filtro OR: estado == "primerrecordatorio" O estado == "segundorecordatorio"
            var filtro = Builders<Factura>.Filter.In(
                f => f.Estado,
                new[] { "primerrecordatorio", "segundorecordatorio" }
            );

            return await _facturasCollection.Find(filtro).ToListAsync();
        }

        /// <summary>
        /// Actualiza el campo "estado" de una factura concreta en MongoDB.
        /// Se llama después de enviar el email para registrar que el proceso ocurrió.
        /// </summary>
        /// <param name="id">El ID único de la factura (ObjectId de MongoDB).</param>
        /// <param name="nuevoEstado">El nuevo estado: "segundorecordatorio" o "desactivado".</param>
        public async Task UpdateEstadoAsync(string id, string nuevoEstado)
        {
            // Filter: encuentra el documento cuyo _id coincida con el id proporcionado
            var filtro = Builders<Factura>.Filter.Eq(f => f.Id, id);

            // Update: solo modifica el campo "Estado", el resto del documento queda intacto
            var actualizacion = Builders<Factura>.Update.Set(f => f.Estado, nuevoEstado);

            await _facturasCollection.UpdateOneAsync(filtro, actualizacion);
        }
    }
}
