namespace Monolegal.API.Models
{
    /// <summary>
    /// Clase de configuración para la conexión con MongoDB.
    ///
    /// PATRÓN "OPTIONS": En lugar de leer la config directamente con IConfiguration,
    /// usamos una clase tipada. Esto es más seguro y más fácil de testear.
    ///
    /// Los valores se cargan automáticamente desde la sección "MongoDbSettings"
    /// del archivo appsettings.json.
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// Cadena de conexión al clúster de MongoDB Atlas.
        /// IMPORTANTE: En producción, esta cadena NO debe estar en appsettings.json.
        /// Debe estar en variables de entorno o en un gestor de secretos (Azure Key Vault, etc).
        /// </summary>
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Nombre de la base de datos dentro del clúster (ej: "MonolegalDB").
        /// </summary>
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// Nombre de la colección donde se guardan las facturas (ej: "Facturas").
        /// En MongoDB, una "colección" es equivalente a una "tabla" en SQL.
        /// </summary>
        public string CollectionName { get; set; } = null!;
    }
}
