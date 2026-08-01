# ESTADO ACTUAL DEL PROYECTO — Backend SGSD (Plandi)

Auditoría técnica al 2026-07-31. Documenta únicamente lo que existe en el código del repositorio `integradora_9no/backend/Plandi.API`.

## 1. Resumen general

- **Motor de base de datos:** SQL Server (paquete `Microsoft.EntityFrameworkCore.SqlServer` 10.0.9). La cadena de conexión solo existe en `Plandi.API/appsettings.Development.json` → `ConnectionStrings:DefaultConnection = "Server=localhost;Database=IntegradoraDb;Trusted_Connection=True;TrustServerCertificate=True;"`. `appsettings.json` no define `ConnectionStrings`.
- **Versión de .NET:** `net10.0` en los 4 proyectos (TFM `<TargetFramework>net10.0</TargetFramework>`). EF Core runtime 10.0.9. Herramienta CLI `dotnet-ef` instalada en la máquina: 10.0.10.
- **Solución:** `Plandi.API.slnx` (formato XML de solución nueva) con 4 proyectos: `Plandi.API`, `Plandi.Dto`, `Plandi.Library`, `Plandi.Services`. No existe ningún `.sln` tradicional.
- **Estado de compilación:** **COMPILA** (`dotnet build Plandi.API.slnx` → "Compilación correcta", 0 errores). Hay **6 advertencias** de nullability, todas en `Plandi.Dto`:
  - `UsuarioResponseDto.cs(9,23)` CS8618 (`Nombre`), `(11,23)` CS8618 (`ApellidoPaterno`), `(15,23)` CS8618 (`Email`).
  - `Enums.cs(65,24)`, `(77,20)`, `(86,20)` CS8603 (posible retorno de referencia nulo en `EConverter`).
- **Estado funcional:** el backend es un esqueleto recién inicializado. Existe **1 solo endpoint real** (`GET api/Usuario/GetAll`), 1 servicio (`UsuarioService`) y 1 DTO (`UsuarioResponseDto`). Las 21 entidades están modeladas y migradas, pero **no tienen controllers ni servicios** salvo Usuario. No hay autenticación ni autorización implementadas.
- **Repositorio limpio:** `git status` sin cambios pendientes. La solución se creó en la rama `plandi-limpio` (commits: "Merge pull request #4 from MKOSSJ/plandi-limpio", "Inicializar solución Plandi").
- Las carpetas `docs/` y `database/` del workspace raíz contienen únicamente archivos `.gitkeep` (sin documentación ni scripts SQL).

## 2. Paquetes y versiones (por proyecto)

| Proyecto | Paquete | Versión |
|---|---|---|
| Plandi.API | Microsoft.EntityFrameworkCore.Design | 10.0.9 |
| Plandi.API | Microsoft.EntityFrameworkCore.SqlServer | 10.0.9 |
| Plandi.API | Swashbuckle.AspNetCore | 10.2.3 |
| Plandi.Library | Microsoft.EntityFrameworkCore | 10.0.9 |
| Plandi.Library | Microsoft.EntityFrameworkCore.Design | 10.0.9 |
| Plandi.Library | Microsoft.EntityFrameworkCore.SqlServer | 10.0.9 |
| Plandi.Dto | (ninguno) | — |
| Plandi.Services | (ninguno) | — |

**Referencias entre proyectos** (verificadas en cada `.csproj`):
- `Plandi.API` → `Plandi.Dto`, `Plandi.Library`, `Plandi.Services`.
- `Plandi.Library` → `Plandi.Dto`.
- `Plandi.Services` → `Plandi.Dto`, `Plandi.Library`.
- `Plandi.Dto` → sin referencias.

Observaciones de consistencia: las versiones EF Core son uniformes (10.0.9 en los tres proyectos). La herramienta CLI `dotnet-ef` instalada es 10.0.10, una revisión distinta a la del runtime 10.0.9. No hay paquetes de JWT, AutoMapper, Serilog, FluentValidation ni proyectos de test en la solución.

## 3. Módulo por módulo (Auth, Administración, Planeaciones, Revisión/Chat)

### 3.1 Auth / Seguridad (Yoset)

- **Entidades existentes** (`Plandi.Library/Models/`): `Usuario.cs`, `Rol.cs`, `UsuarioRol.cs`. Todas las entidades de negocio heredan de `BaseEntity` (`Id` long PK, `PublicId` Guid, `Activo`, `CreatedAt`, `UpdatedAt`, `DeletedAt`); `UsuarioRol` es puente sin herencia.
- **DTOs implementados:** 1 — `Plandi.Dto/UsuarioResponseDto.cs` con campos: `Nombre`, `ApellidoPaterno`, `ApellidoMaterno?`, `Email`, `Telefono?`, `UltimoAcceso?`. No expone `Id` ni `PublicId`.
- **Servicios implementados:** `Plandi.Services/Interfaces/IUsuarioService.cs` con `Task<IEnumerable<UsuarioResponseDto>> GetAllUsers()`; implementación `Plandi.Services/UsuarioService.cs` (mismo método único, usa `AsNoTracking()` y proyección LINQ).
- **Endpoints REST implementados:**
  - `GET api/Usuario/GetAll` → `UsuarioController.GetAll()` (`Plandi.API/Controllers/UsuarioController.cs:26`) → invoca `_usuarioService.GetAllUsers()`.
- **Reglas de negocio codificadas:** ninguna a nivel de servicio. Solo configuración de EF en `AppDbContext.ConfigureUsuarios`: índice único sobre `Usuario.Email`, longitudes máximas (`Nombre`/`ApellidoPaterno` 100, `ApellidoMaterno` 100, `Email` 150, `PasswordHash` 500, `Telefono` 30), `Rol.Nombre` único. `Utils.EncryptPassword` (SHA256, sin salt) existe en `Plandi.Dto/Utils/Utils.cs:11` pero **no tiene ningún llamador**.
- **Qué falta de lo planeado originalmente:** todo lo de autenticación real. No hay login, registro, JWT, refresh token, `[Authorize]`, 2FA ni recuperación de contraseña en ningún archivo (ver sección 5).

### 3.2 Administración / Catálogos (Marco)

- **Entidades existentes** (`Plandi.Library/Models/`): `Carrera.cs`, `Asignatura.cs`, `Academia.cs`, `AcademiaUsuario.cs`, `CicloEscolar.cs`, `Periodo.cs`, `Grupo.cs`, `CargaAcademica.cs`. **Todas existen.** `AcademiaUsuario` es puente sin herencia de `BaseEntity`.
- **DTOs implementados:** ninguno.
- **Servicios implementados:** ninguno.
- **Endpoints REST implementados:** ninguno.
- **Reglas de negocio codificadas:** únicamente en `AppDbContext.ConfigureAcademico`/`ConfigureAcademias` (fluent API): índices únicos en `Carrera.Clave`, `Asignatura.Clave`, `Academia.Nombre`, `CicloEscolar.Nombre`, compuestos `(CicloEscolarId, Nombre)` en `Periodo`, `(PeriodoId, Nombre)` en `Grupo`, `(PeriodoId, GrupoId, AsignaturaId, DocenteId)` en `CargaAcademica`. FKs con `OnDelete(DeleteBehavior.Restrict)`. `CargaAcademica.CreatedBy` (`long?`) no tiene relación configurada (no es FK).
- **Qué falta de lo planeado originalmente:** todo el CRUD/API de catálogos. Para Carrera, Asignatura, Academia, AcademiaUsuario, CicloEscolar, Periodo, Grupo y CargaAcademica solo existen la entidad, la tabla y el seed; no hay controller, servicio, interfaz ni DTO.

### 3.3 Planeaciones Didácticas (Shaolin)

- **Entidades existentes** (`Plandi.Library/Models/`): `PlaneacionDidactica.cs`, `PlaneacionDocente.cs`, `PlaneacionGrupo.cs`, `PlaneacionUnidad.cs`, `PlaneacionActividad.cs`, `ProgramaAsignatura.cs`, `Documento.cs`. (`PlaneacionObservacion.cs` existe también y se listó dentro del módulo de Revisión por su rol). Puentes sin herencia: `PlaneacionDocente`, `PlaneacionGrupo`.
- **DTOs implementados:** ninguno.
- **Servicios implementados:** ninguno.
- **Endpoints REST implementados:** ninguno.
- **Reglas de negocio codificadas:** en `AppDbContext.ConfigurePlaneaciones`/`ConfigureDocumentos`: índice único `(PeriodoId, AsignaturaId)` en `PlaneacionDidactica` (una planeación por asignatura-período), único `(PlaneacionDidacticaId, Orden)` en `PlaneacionUnidad`, índice no único `(PlaneacionUnidadId, Orden)` en `PlaneacionActividad`. `EstadoPlaneacion` se persiste como string (`HasConversion<string>()`). `ProgramaAsignatura.Documento` es relación 1:1 (FK `DocumentoId`). `Documento.Estado` y `TipoDocumento` con `HasConversion<string>()` e índices. `Documento.HashSha256` con índice (no único).
- **Qué falta de lo planeado originalmente:** toda la API de planeaciones (CRUD, borradores, envío a revisión), el flujo de subida de documento → extracción → `ProgramaAsignatura`, y la generación del documento de planeación. No hay ningún controller ni servicio.

### 3.4 Revisión / Autorización / Chat (Jerry)

- **Entidades existentes** (`Plandi.Library/Models/`): `chat.cs` (clase `Chat`), `ChatParticipante.cs`, `ChatMensaje.cs`, `PlaneacionObservacion.cs`. Puentes sin herencia: `ChatParticipante`.
- **DTOs implementados:** ninguno.
- **Servicios implementados:** ninguno.
- **Endpoints REST implementados:** ninguno.
- **Reglas de negocio codificadas:** en `AppDbContext.ConfigureChat`: `ChatParticipante.RolEnChat` string requerido (máx. 50), `ChatMensaje.TipoMensaje` string requerido (máx. 50), índices en `ChatMensaje.ChatId` y `ChatMensaje.CreatedAt`. En `PlaneacionObservacion`: campo `Estado` string con valor por defecto `"ABIERTA"` en el modelo, y `Estado` persistido como `nvarchar(50)`. Tipos usados como string (no enum): `ChatMensaje.TipoMensaje` = `"TEXTO"`/`"OBSERVACION"`, `ChatParticipante.RolEnChat` = `"DOCENTE"`/`"REVISOR"` (valores vistos en `DataSeeder`).
- **Qué falta de lo planeado originalmente:** toda la API de revisión (asignación de revisores, aprobación/rechazo, observaciones) y todo el módulo de chat (envío/lectura de mensajes, participantes). No hay controller ni servicio.

## 4. Convenciones reales detectadas en el código

- **Formato de respuesta:** **no existe un wrapper común**. El único endpoint real (`UsuarioController.GetAll`) responde `Ok(users)` (lista JSON plana, 200). El único manejo de error existente devuelve `StatusCode(500, new { message = "$_Excepcion_Ocurrida" })` (`UsuarioController.cs:37`) — nótese que es un **string literal** cuyo contenido es `$_Excepcion_Ocurrida` (placeholder tipo archivo de recursos que se devuelve textualmente al cliente).
- **Mapeo Entity → DTO:** **manual por proyección LINQ** (`UsuarioService.GetAllUsers`, `Select(u => new UsuarioResponseDto {...})`). No hay AutoMapper ni ningún otro perfil de mapeo en la solución. Al ser el único servicio, no aplica "inconsistencia entre módulos", pero sí es el único patrón existente.
- **Manejo de errores de negocio:** no existe excepción de negocio personalizada, no hay `ProblemDetails`, no hay middleware global de errores. El único caso es `try/catch` dentro del controller con `_logger.LogError` + `StatusCode(500)`. No es posible hablar de consistencia: solo hay un punto de manejo.
- **Convención de nombres de archivos/carpetas:** entidades en `Plandi.Library/Models/` (una clase por archivo, PascalCase) con **una excepción real: `Models/chat.cs` está en minúsculas** (contiene la clase `Chat`). DTOs en la raíz de `Plandi.Dto` (no hay subcarpeta por módulo). Enums agrupados todos en `Plandi.Dto/Enums/Enums.cs`. Utilidades en `Plandi.Dto/Utils/Utils.cs`. Interfaces de servicio en `Plandi.Services/Interfaces/`, clases de servicio en la raíz de `Plandi.Services`. Controllers en `Plandi.API/Controllers/`.
- **Identificador público:** `BaseEntity` define `PublicId` (`Guid`, `= Guid.NewGuid()`) y todas las entidades de negocio lo heredan. **Excepciones reales:** las entidades puente `UsuarioRol`, `AcademiaUsuario`, `PlaneacionDocente`, `PlaneacionGrupo` y `ChatParticipante` **no heredan de `BaseEntity`** y no tienen `PublicId`. En los DTOs, el único existente (`UsuarioResponseDto`) no expone `PublicId` ni `Id`.
- **Estilos de código divergentes dentro de la solución:** la mayoría de archivos usan `namespace X { }` con llaves (bloques), pero `AppDbContext.cs` y `DataSeeder.cs` usan `namespace Plandi.Library.Models;` de estilo *file-scoped*. Además hay `using` sin usar: `Microsoft.Identity.Client` en `UsuarioController.cs:3`, `System.Data` en `IUsuarioService.cs:3` y `UsuarioService.cs:5`.

## 5. Autenticación y autorización (estado real)

- **JWT:** **no implementado.** No existe el paquete `Microsoft.AspNetCore.Authentication.JwtBearer`, ni generación/validación de tokens, ni configuración de clave/firma en `appsettings`. Búsqueda en todo el backend: sin coincidencias de `Jwt`, `Bearer`, `Token`, `AddAuthentication`, `UseAuthentication`.
- **Roles:** solo a nivel de datos. Existen la entidad `Rol` y la relación `UsuarioRol`, y `DataSeeder.SeedRoles` inserta 4 roles: `Administrador` (1), `Docente` (2), `Revisor` (3), `Director` (4). **No se usan** en ningún `[Authorize(Roles = ...)]` (no hay ningún `[Authorize]` en la solución).
- **`[Authorize]`:** ninguna ocurrencia en ningún controller.
- **Middleware:** `Program.cs:42` llama `app.UseAuthorization()`, pero **no** hay `builder.Services.AddAuthentication(...)` ni `UseAuthentication()`, por lo que la autorización no tiene proveedor de identidad; el endpoint queda accesible sin credenciales.
- **2FA:** no encontrado.
- **Recuperación de contraseña:** no encontrado.
- **Hashing de contraseñas:** `Utils.EncryptPassword` (SHA256 en `Plandi.Dto/Utils/Utils.cs:11`) está definido pero sin llamadores. En `DataSeeder`, todos los usuarios semilla usan `PasswordHash = "DEV_HASH_SOLO_PRUEBAS"`.
- **Enums de roles de academia:** `Plandi.Dto/Enums/Enums.cs` define `RolAcademia` (`Docente=1, Revisor=2, JefeAcademia=3, Coordinador=4`) usado por `AcademiaUsuario.RolEnAcademia`, también sin consumo por servicios.

## 6. Inconsistencias y riesgos detectados

1. **`Program.cs` con código duplicado y basura:** punto y coma suelto en la línea 5; `AddEndpointsApiExplorer()` + `AddSwaggerGen()` registrados dos veces (líneas 14-15 y 26-27); Swagger habilitado dos veces: condicional `if (app.Environment.IsDevelopment())` (líneas 31-35) **y** además incondicional (líneas 37-38), lo que deja Swagger expuesto en producción.
2. **Controller con dependencia inyectada sin usar:** `UsuarioController` inyecta `AppDbContext` (`_dBContext`) en el constructor y nunca lo utiliza.
3. **Respuesta de error textualmente rota:** `StatusCode(500, new { message = "$_Excepcion_Ocurrida" })` devuelve al cliente el texto literal `$_Excepcion_Ocurrida` (placeholder sin resolver), no un mensaje real ni el mensaje de excepción.
4. **Manejo inconsistente de tipado de enums:** `EstadoPlaneacion`, `TipoDocumento`, `EstadoDocumento` y `RolAcademia` son enums (persistidos como string), mientras que `ChatMensaje.TipoMensaje`, `ChatParticipante.RolEnChat` y `PlaneacionObservacion.Estado` son `string` con valores "mágicos" (`"TEXTO"`, `"OBSERVACION"`, `"ABIERTA"`). No hay enums `TipoMensaje`, `RolEnChat` ni `EstadoObservacion`.
5. **Nombre de archivo fuera de convención:** `Models/chat.cs` en minúsculas (único archivo así).
6. **`EConverter` con métodos potencialmente nulos:** 3 advertencias CS8603 (`Enums.cs:65,77,86`) y el método genérico `GetEnumFromValue<T>(string)` compara `Enum.IsDefined(typeof(T), value)` contra un string (el overload de `IsDefined(Type, object)` con string no coincide con el nombre del miembro a menos que sea el valor numérico), comportamiento dudoso.
7. **`UsuarioResponseDto` sin inicializar:** propiedades no-nullable sin valor inicial (`Nombre`, `ApellidoPaterno`, `Email`) → 3× CS8618.
8. **Fechas de creación de entidades puente:** `UsuarioRol`, `AcademiaUsuario`, `PlaneacionDocente`, `PlaneacionGrupo`, `ChatParticipante` definen `CreatedAt` propio en vez de heredar de `BaseEntity`, y no tienen `PublicId` (inconsistencia con el resto del modelo).
9. **Campos `CreatedBy` sin relación EF:** `CargaAcademica.CreatedBy` (`long?`) y `PlaneacionDidactica.CreatedBy` (`long?`) no tienen FK configurada (no apuntan a `Usuario`), a diferencia de `UpdatedBy` en `PlaneacionActividad`.
10. **Seed con contraseñas de prueba:** `DataSeeder` persiste `PasswordHash = "DEV_HASH_SOLO_PRUEBAS"` para 4 usuarios; si la base se usa fuera de desarrollo, hay usuarios accesibles con hash inválido.
11. **Sin proyecto de pruebas:** no hay ningún proyecto de test en la solución.
12. **Versionado de la herramienta:** `dotnet-ef` instalada 10.0.10 vs runtime EF 10.0.9 (menor riesgo, pero versión distinta).
13. **Rutas no uniformes:** `UsuarioController` usa `[Route("api/[controller]")]` → `api/Usuario`, mientras `WeatherForecastController` (plantilla sin limpiar) usa `[Route("[controller]")]` → `/WeatherForecast`, sin prefijo `api`.
14. **Sin CORS:** `Program.cs` no registra ningún `AddCors`/`UseCors`.

## 7. Migraciones de base de datos

- **Migraciones existentes (en orden cronológico):**
  1. `20260710055858_InitialCreate` (`Plandi.Library/Migrations/20260710055858_InitialCreate.cs`, generada el 2026-07-10).
- **Snapshot del modelo:** `Plandi.Library/Migrations/AppDbContextModelSnapshot.cs` con `ProductVersion = "10.0.9"`.
- **Contenido de la migración:** crea **22 tablas** (`academias`, `carreras`, `ciclos_escolares`, `roles`, `usuarios`, `asignaturas`, `periodos`, `academia_usuarios`, `documentos`, `usuario_roles`, `grupos`, `programas_asignatura`, `carga_academica`, `planeaciones_didacticas`, `chats`, `planeacion_docentes`, `planeacion_grupos`, `planeacion_unidades`, `chat_mensajes`, `chat_participantes`, `planeacion_actividades`, `planeacion_observaciones`) e inserta todo el seed (`DataSeeder`) dentro del propio `Up()`. No existe una migración de seed separada.
- **Coincidencia con el modelo actual:** **SÍ coincide.** Ejecutando `dotnet ef migrations has-pending-model-changes --project Plandi.Library --startup-project Plandi.API` devuelve: *"No changes have been made to the model since the last migration."*
- **Estado de aplicación:** `dotnet ef migrations list` se conectó a la base local `IntegradoraDb` (SQL Server en `localhost`) y confirmó que **la única migración `20260710055858_InitialCreate` ya está aplicada** (`__EFMigrationsHistory` presente con esa entrada).
# ESTADO REAL DEL BACKEND SGSD — Resumen para trabajo de Marco (Admin/Catálogos)

> Versión condensada de la auditoría del 2026-07-31. Pegar este archivo en vez de la auditoría completa para dar contexto rápido.

## Stack confirmado (no volver a discutir)
- .NET **10.0** en los 4 proyectos. EF Core **10.0.9** (Microsoft.EntityFrameworkCore + .Design + .SqlServer) — versiones ya alineadas, sin conflicto.
- **SQL Server** (no MySQL, no Pomelo). Cadena de conexión solo en `Plandi.API/appsettings.Development.json`.
- Solución: `Plandi.API.slnx`, 4 proyectos: **Plandi.API**, `Plandi.Dto`, `Plandi.Library`, `Plandi.Services`.
- ⚠️ **El proyecto de API se llama `Plandi.API` (mayúsculas), no `Plandi.Api`.** Namespace real de controllers: `Plandi.API.Controllers`.
- Migración única `20260710055858_InitialCreate`, ya aplicada en `IntegradoraDb` local. **Coincide con el modelo actual** (sin cambios pendientes). Las 22 tablas y las 8 entidades de catálogo (Carrera, Asignatura, Academia, AcademiaUsuario, CicloEscolar, Periodo, Grupo, CargaAcademica) **ya existen en `Plandi.Library/Models` y ya están migradas** — no se toca `AppDbContext` para el trabajo de catálogos, ya está todo configurado ahí (índices únicos, FKs, etc.).
- Compila sin errores, 6 warnings de nullability menores en `Plandi.Dto` (no relacionados a catálogos).

## Convenciones reales ya establecidas en el repo (seguir, no inventar otras)
- Entidades: `Plandi.Library/Models/`, una clase por archivo, PascalCase.
- DTOs: raíz de `Plandi.Dto` (plano, sin subcarpeta por módulo — único DTO existente hoy es `UsuarioResponseDto.cs`).
- Enums: todos juntos en `Plandi.Dto/Enums/Enums.cs` (ya incluye `RolAcademia` con 4 valores: Docente=1, Revisor=2, JefeAcademia=3, Coordinador=4 — no crear otro).
- Interfaces de servicio: `Plandi.Services/Interfaces/`. Clases de servicio: raíz de `Plandi.Services`.
- Controllers: `Plandi.API/Controllers/`.
- Identificador público: `BaseEntity.PublicId` (Guid). Las tablas puente (`UsuarioRol`, `AcademiaUsuario`, `PlaneacionDocente`, `PlaneacionGrupo`, `ChatParticipante`) **no** tienen PublicId (llave compuesta).
- **No existe todavía ningún wrapper de respuesta ni excepción de negocio en el repo** — el único endpoint real (`UsuarioController`) responde `Ok(lista)` plano. El wrapper `ApiResponse<T>` que definimos es una convención **nueva que Marco introduce primero**; no tocar `UsuarioController` para "corregirlo" — eso es de Yoset.

## Riesgos/bugs ya detectados que NO son responsabilidad de Marco arreglar ahora
- `Program.cs` tiene Swagger duplicado y expuesto también en producción (bug de Yoset/setup inicial). **Avisar al equipo, no tocarlo sin acuerdo.**
- `UsuarioController` inyecta `AppDbContext` sin usarlo, y su catch devuelve un string placeholder roto (`"$_Excepcion_Ocurrida"`).
- Convención de rutas inconsistente: `UsuarioController` usa singular (`api/Usuario`); catálogos de Marco usarán plural (`api/Carreras`) por ser más estándar REST — **queda pendiente de decisión de equipo**, no bloquea el trabajo de Marco.

## Inventario del módulo de Marco (Admin/Catálogos) — estado real
| Entidad | Existe en Models | DTO | Service | Controller |
|---|---|---|---|---|
| Carrera | ✅ | ✅ (hecho) | ✅ (hecho) | ✅ (hecho) |
| Asignatura | ✅ | ✅ (hecho) | ✅ (hecho) | ✅ (hecho) |
| Academia + AcademiaUsuario | ✅ | ✅ (hecho) | ✅ (hecho) | ✅ (hecho) |
| CicloEscolar | ✅ | ✅ (hecho) | ✅ (hecho) | ✅ (hecho) |
| Periodo | ✅ | ✅ (hecho) | ✅ (hecho) | ✅ (hecho) |
| Grupo | ✅ | ✅ (hecho) | ✅ (hecho) | ✅ (hecho) |
| CargaAcademica | ✅ | ✅ (hecho) | ✅ (hecho) | ⬜ pendiente (falta el controller) |

Todo lo listado abajo está **pendiente de crear** — nada de esto existe todavía en el repo salvo lo que ya está en la tabla de `Models`.

## TAREAS A REALIZAR (en este orden, una rama por bloque)

### Reglas fijas para todas las tareas (no negociables)
- Namespace de controllers: `Plandi.API.Controllers` (el proyecto se llama `Plandi.API`, con mayúsculas).
- DTOs en `Plandi.Dto/Catalogos/` (nueva subcarpeta, un archivo por entidad: `{Entidad}Dtos.cs` con `{Entidad}RequestDto` y `{Entidad}ResponseDto`).
- Interfaces de servicio en `Plandi.Services/Interfaces/I{Entidad}Service.cs`. Clases de servicio en `Plandi.Services/{Entidad}Service.cs` (raíz, sin subcarpeta).
- Controllers en `Plandi.API/Controllers/{Entidad}sController.cs`, ruta `[Route("api/[controller]")]` (plural, ej. `api/Carreras`).
- Crear también, una sola vez, en `Plandi.Dto/Common/`: `ApiResponse.cs` (wrapper genérico `{ success, data, message, errors }` con métodos estáticos `Ok(...)` y `Fail(...)`) y `AppException.cs` (excepción simple de negocio con un constructor `(string message)`).
- Todo endpoint responde envuelto en `ApiResponse<T>`.
- Mapeo Entity→DTO: manual, método privado estático `ToDto(...)` dentro de cada servicio. **Nunca** un `.Select(x => ToDto(x))` dentro de una consulta EF Core (no es traducible a SQL) — primero `.ToListAsync()`, luego `.Select(ToDto).ToList()` en memoria.
- Todas las relaciones en los DTOs de entrada/salida usan `PublicId` (Guid), nunca el `Id` interno (long). Los servicios resuelven `PublicId → Id` internamente antes de tocar la base de datos.
- Borrado = lógico (`Activo = false`, `DeletedAt = DateTime.UtcNow`), nunca `Remove()`.
- Validaciones de formato con Data Annotations en los DTOs (`[Required]`, `[MaxLength]`, `[Range]`). Validaciones de negocio (unicidad, existencia de relaciones, fechas) lanzando `AppException` desde el servicio, capturada en el controller como `Conflict(ApiResponse<T>.Fail(ex.Message))`.
- **No modificar** `AppDbContext.cs` (ya tiene todo configurado para estas 8 entidades), ni `UsuarioController.cs`, ni nada de Auth. Si hace falta tocar `Program.cs`, solo agregar líneas nuevas de `builder.Services.AddScoped<I...Service, ...Service>();`, no tocar lo demás del archivo.

### Bloque 1 — rama `feature/admin-catalogos-base`
Implementar CRUD completo (GET all, GET by publicId, POST, PUT, DELETE lógico) para:
1. **Carrera** (Clave único, Nombre, Nivel).
2. **Asignatura** (Clave único, Nombre, Cuatrimestre, HorasTotales, HorasSemana, Creditos, relación opcional a Academia por `AcademiaPublicId`).
3. **CicloEscolar** (Nombre único, FechaInicio, FechaFin — validar FechaFin > FechaInicio).
4. **Periodo** (depende de CicloEscolar por `CicloEscolarPublicId`; Nombre único dentro del mismo ciclo; validar FechaFin > FechaInicio).
5. **Grupo** (depende de Carrera y Periodo por `CarreraPublicId`/`PeriodoPublicId`; Nombre único dentro del mismo periodo; Cuatrimestre).

### Bloque 2 — rama `feature/admin-academia-carga` (después de mergear el Bloque 1 a develop)
6. **Academia** (Nombre único, Descripcion) + sub-recurso **AcademiaUsuario**: endpoints para listar usuarios de una academia (`GET /api/academias/{publicId}/usuarios`), asignar uno (`POST /api/academias/{publicId}/usuarios` con `UsuarioPublicId` + `RolEnAcademia` usando el enum ya existente `Plandi.Dto.Enums.RolAcademia`), y desasignar (`DELETE /api/academias/{publicId}/usuarios/{usuarioPublicId}`, borrado lógico del vínculo).
7. **CargaAcademica**: depende de Periodo, Grupo, Asignatura, Usuario (Docente obligatorio, Revisor opcional), Academia (opcional). Validar que no exista ya una carga idéntica (mismo Periodo+Grupo+Asignatura+Docente) antes de crear/actualizar.

### Al terminar cada bloque
- `dotnet build` debe compilar sin errores nuevos.
- Registrar cada servicio nuevo en `Program.cs` (solo agregar líneas, no tocar el resto).
- Un solo PR por bloque hacia `develop`.