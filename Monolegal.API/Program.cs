using Monolegal.API.Models;
using Monolegal.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// SERVICIOS DE LA API
// ─────────────────────────────────────────────────────────────────────────────

// Habilita los controladores (FacturasController, etc.)
builder.Services.AddControllers();

// Necesario para que Swagger pueda descubrir y documentar los endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────────────────────────────────────────────────────────────────────
// CONFIGURACIÓN TIPADA (PATRÓN OPTIONS)
// En lugar de leer IConfiguration directamente, registramos clases tipadas.
// Esto permite validar la config al arrancar y hace el código más limpio.
// ─────────────────────────────────────────────────────────────────────────────

// Lee la sección "MongoDbSettings" del appsettings.json y la mapea a la clase
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Lee la sección "EmailSettings" del appsettings.json y la mapea a la clase
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// ─────────────────────────────────────────────────────────────────────────────
// INYECCIÓN DE DEPENDENCIAS — NUESTROS SERVICIOS
// ─────────────────────────────────────────────────────────────────────────────

// SINGLETON: Se crea una sola instancia y se reutiliza en toda la aplicación.
// Correcto para FacturaService porque la conexión a MongoDB es cara de crear
// y el driver de MongoDB es thread-safe (seguro usar desde varios hilos).
builder.Services.AddSingleton<FacturaService>();

// TRANSIENT: Se crea una instancia nueva cada vez que se necesita.
// Correcto para EmailService porque cada envío es una operación independiente.
// IEmailService es la interfaz; EmailService es la implementación concreta.
builder.Services.AddTransient<IEmailService, EmailService>();

// ─────────────────────────────────────────────────────────────────────────────
// CORS (Cross-Origin Resource Sharing)
// Permite que Angular (que corre en otro puerto) pueda llamar a esta API.
// SEGURIDAD: En producción, sustituir AllowAnyOrigin() por la URL exacta
// del frontend: .WithOrigins("https://tu-app.com")
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ─────────────────────────────────────────────────────────────────────────────
// PIPELINE HTTP — ORDEN IMPORTA
// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Swagger solo en desarrollo — nunca exponer en producción
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Interfaz visual en /swagger
}

// CORS debe ir antes de los controllers
app.UseCors("AllowAngular");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

// Conecta las URLs de la API con los métodos de los controladores
app.MapControllers();

app.Run();
