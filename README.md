# Aurora Hoteles · Development Test NTS

Solución para gestionar la búsqueda y calificación de una cadena de hoteles:

- **`backend/`** – API REST en **.NET 8** con CRUD de hoteles protegido para el administrador y listado/filtro público por nombre y/o calificación.
- **`frontend/`** – Aplicación **Angular 19** responsiva con una landing page para usuarios y un panel de administración.

## Requisitos

- .NET SDK 8.0
- Node.js 20+ (probado con Node 22) y npm

## Ejecución rápida (desarrollo)

```bash
# 1. API  →  http://localhost:5080  (Swagger en /swagger)
cd backend/src/HotelChain.Api
dotnet run

# 2. Frontend  →  http://localhost:4200
cd frontend
npm install
npm start
```

En desarrollo la API usa **SQLite** (`hotelchain.dev.db`), crea el esquema automáticamente y carga 8 hoteles de ejemplo.

**Credenciales de administrador (solo desarrollo):** `admin` / `Admin123!`

## Ejercicio 1 · API REST

### Arquitectura

Arquitectura por capas (Clean Architecture), donde las dependencias apuntan hacia el dominio:

```
backend/
├── src/
│   ├── HotelChain.Domain          Entidades (Hotel) e interfaces de repositorio. Sin dependencias.
│   ├── HotelChain.Application     Casos de uso (HotelService), DTOs, validaciones, excepciones de negocio.
│   ├── HotelChain.Infrastructure  EF Core (DbContext, configuración), repositorios, autenticación JWT.
│   └── HotelChain.Api             Controladores, middleware de errores, configuración por entorno.
└── tests/
    └── HotelChain.Tests           Pruebas unitarias del servicio e integración de la API (xUnit).
```

Decisiones relevantes para **escalabilidad, reutilización y separación de responsabilidades**:

- **Repositorio genérico** (`IRepository<T>` / `EfRepository<T>`) reutilizable por cualquier entidad nueva; `HotelRepository` solo agrega la búsqueda específica.
- La lógica de negocio (nombre único, validación de filtros, fechas de auditoría) vive en `HotelService`, independiente de HTTP y de la base de datos.
- **Proveedor de base de datos configurable** (`Sqlite`, `SqlServer`, `InMemory`) sin cambiar código.
- **Paginación** en el listado para soportar catálogos grandes; índices en `Name` (único) y `Rating`.
- Errores homogéneos en formato **ProblemDetails (RFC 7807)** mediante un middleware central.
- Cada capa se registra con su propio método de extensión (`AddApplication`, `AddInfrastructure`).

### Endpoints

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | Devuelve un JWT para el administrador |
| `GET` | `/api/hotels` | Público | Lista y filtra hoteles (paginado) |
| `GET` | `/api/hotels/{id}` | Público | Detalle de un hotel |
| `POST` | `/api/hotels` | **Admin** | Crea un hotel |
| `PUT` | `/api/hotels/{id}` | **Admin** | Actualiza un hotel |
| `DELETE` | `/api/hotels/{id}` | **Admin** | Elimina un hotel |
| `GET` | `/health` | Público | Health check |

Parámetros de filtro de `GET /api/hotels` (todos opcionales y combinables):

| Parámetro | Ejemplo | Descripción |
|---|---|---|
| `name` | `aurora` | Coincidencia parcial, sin distinguir mayúsculas |
| `rating` | `5` | Calificación exacta (1–5) |
| `minRating` | `4` | Calificación mínima (1–5); no se combina con `rating` |
| `page`, `pageSize` | `1`, `20` | Paginación (`pageSize` máx. 100) |

```bash
curl "http://localhost:5080/api/hotels?name=aurora&minRating=4"

TOKEN=$(curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r .token)

curl -X POST http://localhost:5080/api/hotels \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Aurora Tunja","city":"Tunja","address":"Calle 20 # 10-5","rating":3,"pricePerNight":180000}'
```

### Configuración por entorno

| Ajuste | Development (`appsettings.Development.json`) | Production (`appsettings.Production.json`) |
|---|---|---|
| Base de datos | SQLite local + datos de ejemplo | SQL Server, sin datos de ejemplo |
| Swagger | Habilitado | Deshabilitado |
| Logging | `Information` (incluye SQL) | `Warning` |
| CORS | `http://localhost:4200` | Dominio configurado |
| HTTPS / HSTS | No | Redirección HTTPS + HSTS |
| Detalle de errores 500 | Se incluye la traza | Mensaje genérico |
| Secretos (JWT, admin) | Valores de prueba | **Variables de entorno obligatorias** |

En producción los secretos nunca van en el repositorio; se definen con variables de entorno (la API no arranca si falta `Jwt:Key`):

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__HotelChain="Server=...;Database=HotelChain;User Id=...;Password=...;TrustServerCertificate=True"
export Jwt__Key="<clave aleatoria de al menos 32 caracteres>"
export AdminUser__Username="<usuario>"
export AdminUser__Password="<contraseña>"
export Cors__AllowedOrigins__0="https://mi-dominio.com"
dotnet HotelChain.Api.dll
```

Para probar el modo producción en local: `dotnet run --launch-profile production-local` (con las variables anteriores).

### Pruebas

```bash
cd backend
dotnet test
```

13 pruebas: filtros por nombre/calificación, nombre duplicado, 404, autorización (401 sin token), validaciones (400) y flujo CRUD completo contra la API.

## Ejercicio 2 · Frontend Angular

```
frontend/src/app/
├── core/          Modelos, servicios HTTP (HotelService, AuthService), interceptor JWT y guard de admin
├── shared/        Componentes reutilizables: barra de búsqueda y calificación en estrellas
└── features/
    ├── public/landing     Landing page para usuarios
    └── admin/             Login, listado con acciones y formulario de crear/editar
```

- **Usuario** (`/`): introducción a la cadena, listado de todos los hoteles en tarjetas y búsqueda por nombre y/o calificación (exacta o mínima), con búsqueda en vivo.
- **Administrador** (`/admin`): inicio de sesión, listado paginado con filtros, crear, editar y eliminar (con confirmación). Las rutas están protegidas por un guard y el token se envía con un interceptor HTTP.
- **Responsivo**: grid fluido para las tarjetas, menú hamburguesa en móvil y la tabla del admin se transforma en tarjetas en pantallas pequeñas.
- Componentes standalone, carga diferida por ruta, signals y formularios reactivos con validación.

La URL de la API se configura por entorno en `src/environments/` (`environment.development.ts` apunta a `http://localhost:5080/api`; en producción se usa `/api` detrás del mismo dominio).

```bash
cd frontend
npm start          # desarrollo
npm run build      # build de producción en dist/
npm test           # pruebas unitarias (Karma)
```
