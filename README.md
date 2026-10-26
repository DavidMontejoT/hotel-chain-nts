# Aurora Hoteles · Development Test NTS

Solución para gestionar la búsqueda y calificación de una cadena de hoteles:

- **`backend/`** – API REST en **.NET 8** con CRUD de hoteles protegido para el administrador y listado público filtrado por nombre y/o calificación **en la base de datos**.
- **`frontend/`** – Aplicación **Angular 19** responsiva con una landing page para usuarios y un panel de administración, ambos consumiendo la API.

## Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 20 o superior (probado con Node 22) y npm. No hace falta instalar Angular CLI de forma global: los scripts de npm usan la versión local del proyecto.
- Opcional: [Docker](https://www.docker.com/) para levantar todo con un solo comando.

## Opción A · Ejecución local (desarrollo)

```bash
# 1. API  →  http://localhost:5080   (Swagger en http://localhost:5080/swagger)
cd backend/src/HotelChain.Api
dotnet run

# 2. Frontend  →  http://localhost:4200   (en otra terminal)
cd frontend
npm install
npm start
```

En desarrollo la API usa **SQLite** (`hotelchain.dev.db`): al arrancar aplica las migraciones y carga 8 hoteles de ejemplo, así que no hay que instalar ninguna base de datos.

**Credenciales de administrador (solo desarrollo):** `admin` / `Admin123!`

## Opción B · Docker (SQL Server + API + frontend)

```bash
docker compose up --build
```

| Servicio | URL |
|---|---|
| Aplicación (Angular servido por nginx) | http://localhost:8080 |
| API | http://localhost:5080 |

La API corre en modo **Production** contra **SQL Server 2022**, aplica las migraciones al iniciar y nginx redirige `/api` y `/uploads` hacia ella. Las fotos subidas se guardan en un volumen. Los secretos de demostración se pueden cambiar copiando `.env.example` a `.env`. En Mac con Apple Silicon, la imagen de SQL Server corre emulada (`linux/amd64`), así que la primera vez tarda un poco más.

## Ejercicio 1 · API REST

### Arquitectura

Arquitectura por capas (Clean Architecture), donde las dependencias apuntan hacia el dominio:

```
backend/
├── src/
│   ├── HotelChain.Domain                Entidad Hotel, normalización de texto e interfaces de repositorio. Sin dependencias.
│   ├── HotelChain.Application           HotelService (reglas de negocio), DTOs con validaciones, excepciones de negocio.
│   ├── HotelChain.Infrastructure        EF Core (DbContext, configuración), repositorios, autenticación JWT.
│   ├── HotelChain.Migrations.Sqlite     Migraciones de EF Core para SQLite (desarrollo).
│   ├── HotelChain.Migrations.SqlServer  Migraciones de EF Core para SQL Server (producción).
│   └── HotelChain.Api                   Controladores, middleware de errores, configuración por entorno.
└── tests/
    └── HotelChain.Tests                 Pruebas unitarias del servicio e integración de la API (xUnit).
```

Flujo de una petición: `HotelsController` → `IHotelService` → `IHotelRepository` → EF Core. Todo se registra por inyección de dependencias con un método de extensión por capa (`AddApplication`, `AddInfrastructure`).

Decisiones relevantes para **escalabilidad, reutilización y separación de responsabilidades**:

- **Filtros en la base de datos**: el repositorio compone un `IQueryable` (`Where` → `OrderBy` → `Skip/Take`) que EF Core traduce a SQL. Nunca se traen todos los hoteles a memoria.
- **Búsqueda sin tildes ni mayúsculas**: cada hotel guarda un `NormalizedName` indexado, así que "bogota" encuentra "Aurora Bogotá Centro" igual en SQLite que en SQL Server. Ese mismo campo garantiza que no haya dos hoteles con el mismo nombre.
- **DTOs separados**: `CreateHotelDto`, `UpdateHotelDto` (validaciones con Data Annotations), `HotelDto` (lectura) y `HotelQueryDto` (filtros). La entidad nunca se expone directamente.
- **Repositorio genérico** (`IRepository<T>` / `EfRepository<T>`) reutilizable por cualquier entidad nueva; `HotelRepository` solo agrega la búsqueda.
- **Proveedor de base de datos configurable** (`Sqlite`, `SqlServer`, `InMemory`) sin cambiar código.
- **Paginación** en el listado para soportar catálogos grandes.
- Errores homogéneos en formato **ProblemDetails (RFC 7807)** con un middleware global, y **logs estructurados** con `ILogger<T>` al crear, editar y eliminar.

### Endpoints

| Método | Ruta | Acceso | Respuestas |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | 200 con JWT · 401 |
| `GET` | `/api/hotels` | Público | 200 (paginado) · 400 |
| `GET` | `/api/hotels/{id}` | Público | 200 · 404 |
| `POST` | `/api/hotels` | **Admin** | 201 · 400 · 401 · 409 (nombre repetido) |
| `PUT` | `/api/hotels/{id}` | **Admin** | 200 · 400 · 401 · 404 · 409 |
| `DELETE` | `/api/hotels/{id}` | **Admin** | 204 · 401 · 404 |
| `POST` | `/api/images` | **Admin** | 201 con la URL de la foto · 400 · 401 |
| `GET` | `/health` | Público | Health check |

Parámetros de filtro de `GET /api/hotels` (todos opcionales y combinables):

| Parámetro | Ejemplo | Descripción |
|---|---|---|
| `name` | `bogota` | Coincidencia parcial, sin distinguir mayúsculas ni tildes |
| `rating` | `5` | Calificación exacta (1–5) |
| `minRating` | `4` | Calificación mínima (1–5); no se combina con `rating` |
| `page`, `pageSize` | `1`, `20` | Paginación (`pageSize` máx. 100) |

```bash
curl "http://localhost:5080/api/hotels?name=aurora&rating=5"

TOKEN=$(curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r .token)

curl -X POST http://localhost:5080/api/hotels \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Hotel Test 5 estrellas","city":"Bogotá","address":"Calle 100 # 10-20","rating":5,"pricePerNight":500000}'
```

También hay una colección de Postman en [`docs/aurora-hoteles.postman_collection.json`](docs/aurora-hoteles.postman_collection.json) con todas las peticiones (filtros, login y CRUD). El login guarda el token en una variable, así que basta con ejecutar "Login administrador" primero.

### Fotos de los hoteles

Un hotel puede tener una URL de imagen externa (`https://...`) o una foto subida desde el panel de administración:

1. El frontend envía el archivo a `POST /api/images` (`multipart/form-data`, campo `file`).
2. La API valida el **contenido real** del archivo (firma JPG, PNG, WEBP o GIF, no solo la extensión) y que no supere 5 MB, lo guarda con un nombre aleatorio y responde `{ "url": "/uploads/<archivo>" }`.
3. Esa URL se guarda en el hotel y la API sirve el archivo como estático en `/uploads`.

Al cambiar o eliminar la foto de un hotel, el archivo anterior se borra. El almacenamiento está detrás de la interfaz `IImageStorage`, así que se puede cambiar por Azure Blob o S3 sin tocar el resto. La carpeta se configura con `ImageStorage:RootPath` (por defecto `uploads/`, ignorada por git), y en Docker se monta como volumen.

### Migraciones (EF Core)

Las migraciones dependen del proveedor, así que hay un proyecto de migraciones para cada uno. Se usa la herramienta `dotnet-ef` como herramienta local del repositorio.

```bash
cd backend
dotnet tool restore

# Aplicar las migraciones manualmente (en desarrollo se aplican solas al arrancar)
dotnet ef database update -p src/HotelChain.Migrations.Sqlite -s src/HotelChain.Api

# Crear una migración nueva después de cambiar el modelo
dotnet ef migrations add <Nombre> -p src/HotelChain.Migrations.Sqlite -s src/HotelChain.Api -o Migrations
Database__Provider=SqlServer ConnectionStrings__HotelChain="Server=...;Database=HotelChain;..." \
  dotnet ef migrations add <Nombre> -p src/HotelChain.Migrations.SqlServer -s src/HotelChain.Api -o Migrations
```

En producción `Database:ApplyMigrations` viene en `false` para aplicar los cambios de esquema de forma controlada (`dotnet ef database update` o un migration bundle). En Docker se activa para que la demo arranque sola.

### Configuración por entorno

| Ajuste | Development (`appsettings.Development.json`) | Production (`appsettings.Production.json`) |
|---|---|---|
| Base de datos | SQLite local, migraciones automáticas y datos de ejemplo | SQL Server, sin datos de ejemplo |
| Swagger | Habilitado | Deshabilitado |
| Logging | `Information` (incluye SQL) | `Warning` (`Information` para la app) |
| CORS | `http://localhost:4200` | Dominio configurado |
| HTTPS / HSTS | No | Redirección HTTPS + HSTS |
| Detalle de errores 500 | Se incluye la traza | Mensaje genérico |
| Secretos (JWT, admin, conexión) | Valores de prueba | **Variables de entorno obligatorias** |

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

22 pruebas: filtros por nombre y calificación (combinados, exacta y mínima), búsqueda sin tildes, nombre duplicado, 404, autorización (401 sin token), validaciones (400), el flujo CRUD completo contra la API y la subida de fotos (archivo válido, archivo falso con extensión de imagen, sin token y limpieza del archivo al borrar el hotel).

## Ejercicio 2 · Frontend Angular

```
frontend/src/app/
├── core/
│   ├── models/         Interfaces (Hotel, filtros, respuesta paginada)
│   ├── services/       HotelService y AuthService (HttpClient), toasts y confirmaciones
│   ├── interceptors/   JWT del administrador y manejo global de errores HTTP
│   └── guards/         Protección de las rutas /admin
├── shared/components/  Barra de búsqueda, estrellas, selector de imagen, toasts y modal de confirmación
├── shared/pipes/       imageSrc: resuelve las fotos subidas contra el origen de la API
└── features/
    ├── public/         public.routes.ts + landing page para usuarios  (/)
    └── admin/          admin.routes.ts + login, tabla de hoteles y formulario crear/editar  (/admin)
```

- **Módulos por feature con lazy loading**: `app.routes.ts` solo declara dos áreas con `loadChildren` (`PUBLIC_ROUTES` y `ADMIN_ROUTES`). Es el equivalente standalone de los feature modules de Angular: cada área es dueña de sus rutas, y el código del admin no se descarga hasta que el usuario navega a `/admin` (el build genera chunks separados `admin-routes`, `hotel-list-component`, `hotel-form-component`...).

- **Usuario** (`/`): introducción a la cadena, todos los hoteles en tarjetas y búsqueda por nombre y/o calificación (exacta o mínima). **Cada cambio en los filtros hace una petición a la API** (`GET /api/hotels?name=...&rating=...`); el frontend no filtra con JavaScript.
- **Administrador** (`/admin`): inicio de sesión, tabla paginada con los mismos filtros, crear y editar con formularios reactivos validados (la imagen se puede **subir desde el computador o pegar como URL**, con vista previa), y eliminar con un modal de confirmación. Las rutas están protegidas por un guard y el token se envía con un interceptor.
- **Manejo de errores**: un interceptor muestra un aviso cuando la API no responde, la sesión expira, el recurso no existe o hay un error del servidor. Los errores de validación (400) y de nombre repetido (409) se muestran en el formulario.
- **Responsivo**: grid fluido para las tarjetas, menú hamburguesa en móvil, y la tabla del admin se convierte en tarjetas en pantallas pequeñas.
- **Estado reactivo**: los listados se alimentan de un `BehaviorSubject` de filtros con `switchMap` (cancela búsquedas viejas) y se pintan con `async` pipe; el listado del admin se refresca solo después de eliminar. Componentes standalone y tipado estricto, sin `any`.

La URL de la API se configura por entorno en `src/environments/`: `environment.development.ts` apunta a `http://localhost:5080/api` y en producción se usa `/api` detrás del mismo dominio (así funciona el nginx de Docker).

```bash
cd frontend
npm start          # desarrollo
npm run build      # build de producción en dist/
npm test           # pruebas unitarias (Karma + Jasmine)
```

27 pruebas con `HttpClient` simulado (`provideHttpClientTesting`): `HotelService` (filtros como query params, GET, POST, PUT y DELETE), el interceptor de errores, las rutas con carga diferida, el formulario (validaciones, botón deshabilitado, POST y PUT), la landing (los filtros viajan a la API como `?name=&rating=` y se muestra lo que responde) y el listado del admin (confirmar, eliminar y refrescar).

## Decisiones de alcance

Todo lo obligatorio (CRUD, filtros en el servidor, API .NET, Angular, panel de administrador, landing con búsqueda, diseño responsivo, separación de responsabilidades y configuración por entorno) se resolvió primero. Los extras son deliberadamente pequeños:

- **Autenticación mínima**: el enunciado distingue al Administrador del Usuario, y sin protección cualquiera podría borrar hoteles. Por eso hay un único usuario administrador configurado por entorno y un token JWT. No hay registro, base de usuarios ni gestión de roles.
- **Docker es opcional**: el proyecto corre sin Docker con `dotnet run` y `npm start` (SQLite, sin instalar nada). `docker compose` solo sirve para ver la configuración de producción con SQL Server.
- **Fotos**: el formulario acepta una URL o un archivo subido. La subida es un endpoint pequeño detrás de la interfaz `IImageStorage`, que valida el tipo real del archivo y un máximo de 5 MB. Así, cambiar a un almacenamiento en la nube sería solo una implementación nueva.
- **Pruebas enfocadas**: unitarias e integración de la API con xUnit, y unitarias del frontend con Karma. Cubren lo que pide la prueba: filtros, validaciones, CRUD y manejo de errores. No hay pruebas E2E.
- **Sin patrones de más**: para una sola entidad no se usan CQRS, MediatR, GraphQL, i18n ni microservicios. Un servicio de aplicación y un repositorio son suficientes.

## Mejoras si tuviera más tiempo

- **Ordenamiento a elección del usuario** (precio, nombre), además del orden actual por calificación de mayor a menor.
- **Búsqueda por ciudad y rango de precio**, siguiendo el mismo patrón de `HotelQueryDto` y `IQueryable`.
- **Fotos en la nube** (Azure Blob Storage o S3) con una nueva implementación de `IImageStorage`, sin tocar el resto del código.
- **Administradores en base de datos**, con contraseñas hasheadas y refresh tokens, en lugar de un único usuario por configuración.
- **Pruebas E2E** con Playwright del flujo de la "Prueba manual sugerida".
- **CI** con GitHub Actions que corra las pruebas y el build en cada push.
- **Caché** del listado público con ETag o caché de salida, para las búsquedas más frecuentes.

## Prueba manual sugerida

1. En **Admin**, crea "Hotel Test 5 estrellas" con 5 estrellas.
2. En la **landing**, busca `test` en minúscula y filtra por 5 estrellas: aparece el hotel.
3. Vuelve a Admin, edítalo y elimínalo confirmando en el modal: la landing deja de mostrarlo.
