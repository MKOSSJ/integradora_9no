# Ciclo de vida académico y repositorio

## Modelo reutilizado

- `Usuario` representa también a docentes y revisores mediante `UsuarioRol`; no existe ni se necesita una entidad `Docente` duplicada.
- `CargaAcademica` relaciona `Periodo`, `Grupo`, `Asignatura`, docente (`Usuario`), revisor y academia.
- `Grupo` pertenece a un `Periodo` y una `Carrera`.
- `Periodo` pertenece a un `CicloEscolar`.
- `PlaneacionDidactica` pertenece a un periodo/asignatura. Los docentes y grupos se obtienen de las cargas académicas de esa combinación.
- Los programas y sus metadatos de almacenamiento continúan en `ProgramaAsignatura`/`Documento`. El PDF de planeación continúa generándose con `IPlaneacionPdfService`; el repositorio no crea copias.
- `Activo`/`DeletedAt` conservan su significado de habilitación/soft delete. El cierre académico se representa separadamente con `Periodo.Estado` y `FechaCierre`.

## Estados y fechas

`EstadoPeriodo` tiene tres valores: `Programado`, `Activo` y `Cerrado`.

- Programado: la fecha de inicio todavía no llega. Es de sólo lectura.
- Activo: la fecha actual está entre inicio y fin, ambas inclusive. Permite operaciones académicas.
- Cerrado: fue cerrado expresamente o la fecha de fin ya pasó. Es de sólo lectura.

Las fechas de inicio/fin existentes son `DateTime` y representan fechas académicas locales. Para no cambiar contratos ni columnas históricas, se comparan por fecha de calendario en `America/Mexico_City` (Windows: `Central Standard Time (Mexico)`); la fecha final es inclusiva. Las marcas técnicas (`CreatedAt`, `UpdatedAt`, `FechaCierre`) continúan en UTC.

`IPeriodoLifecycleService` es la regla reutilizable. Cada mutación académica de carga, grupo, planeación, secuencias/temas/evaluaciones/referencias, revisión y comentarios exige que el periodo sea editable. Un periodo vencido se bloquea en tiempo real aunque el estado persistido aún diga `Activo`, con respuesta `409 Conflict` y el mensaje: `El periodo se encuentra cerrado y la información ya no puede modificarse.`

`PeriodoClosingHostedService` ejecuta la actualización física al iniciar la API y después cada hora. La migración también clasifica los periodos existentes al instalarse.

## Administración

Todas las rutas siguientes requieren JWT y rol `Director`. Las colecciones aceptan `page` (por defecto 1), `pageSize` (por defecto 20, máximo 100) y `search`.

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/admin/usuarios` | Usuarios con roles, academias y total de cargas. |
| GET | `/api/admin/usuarios/{id}` | Detalle relacionado de usuario. |
| GET | `/api/admin/grupos` | Grupos con carrera, periodo, ciclo y asignaciones humanas. |
| GET | `/api/admin/grupos/{id}` | Detalle relacionado de grupo. |
| GET | `/api/admin/asignaturas` | Asignaturas con academia, programas e imparticiones. |
| GET | `/api/admin/asignaturas/{id}` | Detalle relacionado de asignatura. |
| GET | `/api/admin/ciclos` | Ciclos con periodos y conteos. |
| GET | `/api/admin/ciclos/{id}` | Detalle de ciclo y periodos. |
| GET | `/api/admin/periodos` | Periodos con estado físico/efectivo y conteos. |
| GET | `/api/admin/periodos/{id}` | Detalle relacionado del periodo. |
| POST | `/api/admin/periodos/{id}/cerrar` | Cierre administrativo explícito e idempotente. |
| GET | `/api/admin/cargas-academicas` | Cargas con docente, usuario, asignatura, grupo, periodo, ciclo, carrera, academia y planeación. |
| GET | `/api/admin/cargas-academicas/{id}` | Detalle relacionado de carga. |
| PUT | `/api/admin/cargas-academicas/{id}/grupo` | Cambio controlado de grupo. Body: `{ "grupoPublicId": "guid" }`. |

El cambio de grupo valida existencia, periodo vigente, pertenencia al mismo periodo, cuatrimestre compatible, academia y duplicidad. La actualización general heredada de carga ya no permite cambiar periodo, asignatura o docente; se conserva la ruta para compatibilidad, pero esos cambios requieren operaciones específicas futuras.

## Repositorio

Las rutas requieren JWT y alguno de los roles efectivos `Director`, `Docente` o `Revisor`. Son únicamente GET. El Director consulta todo; el docente sólo combinaciones periodo/asignatura de sus cargas y el revisor sólo sus planeaciones asignadas.

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/repositorio/planeaciones` | Búsqueda histórica paginada. |
| GET | `/api/repositorio/planeaciones/{id}` | Detalle histórico relacionado. |
| GET | `/api/repositorio/planeaciones/{id}/archivos` | Archivos disponibles. |
| GET | `/api/repositorio/planeaciones/{id}/archivos/{tipo}/descargar` | Descarga; `tipo` es `planeacion` o `programa`. |

Filtros combinables: `periodoPublicId`, `asignaturaPublicId`, `docentePublicId`, `cicloPublicId`, `grupoPublicId`, `carreraPublicId`, `academiaPublicId`, `estadoPlaneacion`, `search`, `page` y `pageSize`.

Antes de descargar se vuelve a validar que la planeación sea histórica, que el usuario tenga acceso y que el programa/documento pertenezca a esa planeación. La respuesta de archivo conserva nombre y MIME type; el catálogo de archivos incluye el tamaño persistido cuando existe.

## Base de datos e índices

La migración `AddAcademicPeriodLifecycle` añade a `periodos`:

- `Estado int NOT NULL`, con valor inicial `Activo` y reclasificación por fechas.
- `FechaCierre datetime2 NULL`.
- Índices sobre `Estado`, `FechaInicio` y `FechaFin`.

Se confirmaron y dejaron explícitos en EF los índices sobre `CargaAcademica.GrupoId`, `AsignaturaId` y `DocenteId`; ya existían por convención de claves foráneas, por lo que la migración no los duplica. También se conserva el índice compuesto único existente por periodo/grupo/asignatura/docente.

## Auditoría

Se reutiliza la auditoría básica existente: `CreatedBy`, `UpdatedBy`, `CreatedAt` y `UpdatedAt`. El cambio de grupo registra actor y fecha. No se agregó aún una tabla de bitácora porque el proyecto no tenía esa infraestructura y se evitaron cambios excesivos. Si se requiere valor anterior/nuevo con cumplimiento formal, el siguiente incremento recomendado es una tabla append-only `AuditoriaAdministrativa` escrita desde el servicio de aplicación, sin alterar las entidades históricas.

## Compatibilidad

Los endpoints de catálogos y planeaciones existentes se conservan. El frontend actual puede seguir utilizándolos; las pantallas administrativas nuevas deben consumir `/api/admin`, y la vista histórica `/api/repositorio`. Las respuestas continúan envueltas en `ApiResponse<T>` y las listas nuevas usan el `PagedResult<T>` existente.
