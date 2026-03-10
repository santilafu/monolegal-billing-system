# Monolegal Billing System

Sistema de facturacion y gestion de recordatorios de pago para servicios legales. Automatiza el flujo de avisos a clientes con facturas pendientes mediante email y un dashboard de seguimiento.

---

## Tabla de contenidos

- [Descripcion](#descripcion)
- [Tecnologias](#tecnologias)
- [Arquitectura](#arquitectura)
- [Requisitos previos](#requisitos-previos)
- [Instalacion y ejecucion](#instalacion-y-ejecucion)
- [Como probar la aplicacion](#como-probar-la-aplicacion)
- [Variables de configuracion](#variables-de-configuracion)
- [Endpoints de la API](#endpoints-de-la-api)
- [Flujo de recordatorios](#flujo-de-recordatorios)
- [Estructura del proyecto](#estructura-del-proyecto)

---

## Descripcion

Monolegal Billing System permite a estudios juridicos:

- Visualizar todas las facturas y su estado actual desde un dashboard web.
- Ejecutar un proceso automatico de recordatorios de pago en 3 etapas.
- Enviar notificaciones por email a los clientes en cada etapa del proceso.

### Estados de una factura

| Estado | Descripcion |
|---|---|
| `primerrecordatorio` | El cliente recibe el primer aviso de pago pendiente |
| `segundorecordatorio` | El cliente recibe un aviso final antes de desactivacion |
| `desactivado` | El acceso al servicio queda suspendido hasta regularizar el pago |
| `pagado` | La factura ha sido abonada, no se procesa |

---

## Tecnologias

### Backend
- [.NET 10](https://dotnet.microsoft.com/) / ASP.NET Core 10
- [MongoDB.Driver 3.7](https://www.mongodb.com/docs/drivers/csharp/) — acceso a datos
- [MailKit 4.15](https://github.com/jstedfast/MailKit) — envio de emails via SMTP
- Swagger / OpenAPI — documentacion interactiva de la API

### Frontend
- [Angular 21](https://angular.dev/) — framework SPA standalone
- TypeScript 5.9
- RxJS 7.8
- zone.js — deteccion de cambios

### Base de datos
- [MongoDB Atlas](https://www.mongodb.com/atlas) — cloud NoSQL

---

## Arquitectura

```
monolegal-frontend  (Angular 21, puerto 4200)
        |
        | HTTP (REST)
        v
Monolegal.API       (ASP.NET Core 10, puerto 5218)
        |
        | MongoDB Driver
        v
MongoDB Atlas       (MonolegalDB > Facturas)
```

---

## Requisitos previos

Asegurate de tener instalado:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js >= 20](https://nodejs.org/) y npm >= 11
- Acceso a internet (la base de datos esta en MongoDB Atlas)

Verifica las versiones:

```bash
dotnet --version   # 10.x.x
node --version     # v20.x.x o superior
npm --version      # 11.x.x
```

---

## Instalacion y ejecucion

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd Monolegal.BillingSystem
```

### 2. Iniciar el backend

```bash
cd Monolegal.API
dotnet run
```

La API quedara disponible en:
- HTTP: `http://localhost:5218`
- Swagger UI: `http://localhost:5218/swagger`

### 3. Iniciar el frontend

Abre una segunda terminal:

```bash
cd monolegal-frontend
npm install       # solo la primera vez
npm start
```

El frontend quedara disponible en: `http://localhost:4200`

> Ambos procesos deben estar corriendo al mismo tiempo.

---

## Como probar la aplicacion

Hay dos formas de probar el sistema: desde el **dashboard web** o desde **Swagger UI**.

### Opcion A — Dashboard web (recomendado)

1. Abre `http://localhost:4200` en el navegador.
2. Verifica que las tarjetas superiores muestran el recuento de facturas por estado.
3. Verifica que la tabla muestra el listado de facturas con nombre, email, monto y estado.
4. Haz clic en **Actualizar** — debe recargar los datos sin errores.
5. Haz clic en **Procesar Recordatorios**:
   - Las facturas en `primerrecordatorio` deben pasar a `segundorecordatorio`.
   - Las facturas en `segundorecordatorio` deben pasar a `desactivado`.
   - Debe aparecer un mensaje verde con el resumen del proceso.
6. Verifica que la tabla refleja los nuevos estados tras el proceso.

### Opcion B — Swagger UI (prueba directa de la API)

Swagger es una interfaz grafica que permite llamar a los endpoints sin necesidad de Postman ni código.

1. Abre `http://localhost:5218/swagger` en el navegador.
2. Verás los dos endpoints disponibles:

**Probar GET /api/facturas:**
- Haz clic en `GET /api/facturas` → **Try it out** → **Execute**.
- Debe devolver un array JSON con todas las facturas.
- Codigo de respuesta esperado: `200 OK`.

**Probar POST /api/facturas/procesar:**
- Haz clic en `POST /api/facturas/procesar` → **Try it out** → **Execute**.
- Debe devolver un JSON con el resumen del proceso.
- Codigo de respuesta esperado: `200 OK`.
- Ejemplo de respuesta exitosa:
```json
{
  "mensaje": "Proceso de recordatorios completado.",
  "totalPendientes": 2,
  "procesadas": 2,
  "fallidas": 0,
  "errores": []
}
```

### Verificar que los emails se enviaron

Los emails se envian a traves de [Mailtrap](https://mailtrap.io/) en desarrollo.

1. Inicia sesion en [mailtrap.io](https://mailtrap.io).
2. Ve a **Email Testing** → **Inboxes**.
3. Deberian aparecer los emails enviados durante el proceso.

### Verificar los datos en MongoDB Atlas

> **Acceso temporal — valido hasta el 17 de marzo de 2026 (1 semana)**
>
> | Campo | Valor |
> |---|---|
> | Usuario | `Monolegal` |
> | Contraseña | `1234` |
> | Cluster | `cluster0.lknqnjv.mongodb.net` |

1. Ve a [cloud.mongodb.com](https://cloud.mongodb.com).
2. Inicia sesion con el usuario `Monolegal` y contraseña `1234`.
3. Entra al cluster → **Browse Collections**.
4. Selecciona `MonolegalDB` → `Facturas`.
5. Verifica que los documentos tienen el campo `estado` actualizado correctamente.

---

## Variables de configuracion

El archivo `appsettings.json` incluye credenciales de desarrollo listas para usar (MongoDB Atlas + Mailtrap). No requiere configuracion adicional para probar el proyecto.

### Acceso a MongoDB Atlas

El proyecto usa un usuario dedicado con acceso limitado a la base de datos `MonolegalDB`:

| Campo | Valor |
|---|---|
| Usuario | `Monolegal` |
| Contraseña | `1234` |
| Base de datos | `MonolegalDB` |
| Coleccion | `Facturas` |

> **Aviso:** Este acceso es temporal y expira el **17 de marzo de 2026**. Pasada esa fecha sera necesario renovar las credenciales en MongoDB Atlas y actualizar el `appsettings.json`.

> Los emails de prueba se capturan en [Mailtrap](https://mailtrap.io/) y no llegan a destinatarios reales.

> En produccion sustituye las credenciales por variables de entorno o un gestor como Azure Key Vault.

---

## Endpoints de la API

| Metodo | Ruta | Descripcion |
|---|---|---|
| `GET` | `/api/facturas` | Devuelve todas las facturas |
| `POST` | `/api/facturas/procesar` | Ejecuta el proceso de recordatorios y actualiza estados |

### Ejemplo de respuesta `GET /api/facturas`

```json
[
  {
    "id": "69af0888d295d04c84b64f9a",
    "clienteId": "C01",
    "nombreCliente": "Empresa A",
    "emailCliente": "empresa@ejemplo.com",
    "monto": 1500.50,
    "estado": "primerrecordatorio",
    "fechaFactura": "2025-01-15T00:00:00"
  }
]
```

### Ejemplo de respuesta `POST /api/facturas/procesar`

```json
{
  "mensaje": "Proceso de recordatorios completado.",
  "totalPendientes": 2,
  "procesadas": 2,
  "fallidas": 0,
  "errores": []
}
```

---

## Flujo de recordatorios

Al llamar a `POST /api/facturas/procesar`:

1. Se obtienen todas las facturas en estado `primerrecordatorio` o `segundorecordatorio`.
2. Para cada factura:
   - Si esta en `primerrecordatorio` → se envia email de segundo aviso y se actualiza a `segundorecordatorio`.
   - Si esta en `segundorecordatorio` → se envia email de aviso de desactivacion y se actualiza a `desactivado`.
3. Se devuelve un resumen con el total procesado y los errores ocurridos.
4. Las facturas en estado `desactivado` o `pagado` se ignoran automaticamente.

---

## Estructura del proyecto

```
Monolegal.BillingSystem/
├── Monolegal.API/
│   ├── Controllers/
│   │   └── FacturasController.cs   # Endpoints REST
│   ├── Models/
│   │   ├── Factura.cs              # Modelo de factura (MongoDB)
│   │   ├── MongoDbSettings.cs      # Configuracion de base de datos
│   │   └── EmailSettings.cs        # Configuracion de email
│   ├── Services/
│   │   ├── FacturaService.cs       # Acceso a datos (patron repositorio)
│   │   ├── IEmailService.cs        # Interfaz del servicio de email
│   │   └── EmailServices.cs        # Implementacion con MailKit
│   ├── Properties/
│   │   └── launchSettings.json     # Perfiles de ejecucion
│   ├── appsettings.json            # Configuracion principal
│   ├── appsettings.Development.json
│   └── Program.cs                  # Startup y DI
│
└── monolegal-frontend/
    ├── src/
    │   └── app/
    │       ├── app.ts              # Componente raiz (logica principal)
    │       ├── app.html            # Template del dashboard
    │       ├── app.css             # Estilos del dashboard
    │       ├── app.config.ts       # Configuracion de DI de Angular
    │       └── factura.service.ts  # Servicio HTTP hacia la API
    ├── angular.json
    └── package.json
```
