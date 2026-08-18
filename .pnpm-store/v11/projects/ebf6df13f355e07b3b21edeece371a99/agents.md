# AGENTS.md

# Plandi - Instrucciones para Codex

## 1. Objetivo del proyecto

Plandi es un sistema académico para la gestión de planeaciones didácticas.

El proyecto tiene:

- Frontend Angular.
- Backend ASP.NET Core.
- SQL Server.
- Autenticación JWT.
- Roles.
- Gestión académica.
- Planeaciones.
- Validación/revisión.
- Visualización de PDFs.

El objetivo actual es conectar completamente el frontend existente con el backend existente.

La interfaz del frontend ya está construida.

NO rediseñar la aplicación.

NO reemplazar componentes existentes innecesariamente.

NO cambiar estilos, layouts o navegación salvo que sea necesario para corregir funcionalidad.

El trabajo principal es sustituir datos mock por datos reales provenientes de la API.


# 2. Estructura del repositorio

La estructura general es:


/
├── frontend/
│
└── backend/
    └── Plandi.API/
        ├── Plandi.API/
        ├── Plandi.Dto/
        ├── Plandi.Library/
        └── Plandi.Services/
        
        Antes de asumir rutas diferentes, inspeccionar el repositorio real.

3. Tecnologías

Frontend:

Angular
Standalone Components
TypeScript
Angular Signals
HttpClient
RxJS
Tailwind CSS
Lucide Angular
pdfjs-dist

Backend:

ASP.NET Core
Entity Framework Core
SQL Server
JWT Bearer Authentication
AutoMapper

Base de datos:

SQL Server
4. Regla principal

EL BACKEND ES LA FUENTE DE VERDAD PARA LOS CONTRATOS DE API.

Antes de conectar cualquier pantalla del frontend:

Buscar el Controller correspondiente.
Identificar el endpoint exacto.
Identificar HTTP method.
Identificar Request DTO.
Identificar Response DTO.
Revisar enums utilizados.
Revisar servicio backend correspondiente.
Después revisar el service Angular.
Después modificar el componente.

Nunca inventar:

endpoints,
propiedades,
DTOs,
enums,
valores de enums,
nombres de roles,
estructuras JSON.

Si frontend y backend no coinciden, adaptar el frontend al contrato real del backend salvo que exista un error evidente en el backend.

Si se detecta un error de contrato en backend, reportarlo antes de modificarlo.

5. Forma de trabajo

NO intentar conectar toda la aplicación en una sola modificación.

Trabajar módulo por módulo.

Para cada módulo:

Inspeccionar backend.
Inspeccionar frontend.
Identificar mocks.
Identificar contratos reales.
Mostrar qué archivos se modificarán.
Implementar cambios.
Compilar frontend.
Compilar backend si fue modificado.
Corregir errores introducidos.
Continuar al siguiente módulo.

No continuar al siguiente módulo mientras el actual no compile.

6. Prohibiciones

No realizar refactors generales sin necesidad.

No cambiar nombres de componentes existentes sin necesidad.

No mover carpetas solo por preferencias arquitectónicas.

No cambiar Tailwind por otra solución CSS.

No cambiar Signals por otra estrategia de estado.

No convertir componentes standalone a NgModules.

No eliminar propiedades de modelos simplemente porque todavía sean mock.

No eliminar funcionalidad visual existente.

No agregar librerías sin justificar primero por qué son necesarias.

No modificar migraciones ni esquema de base de datos sin autorización explícita.

No ejecutar:

dotnet ef database drop

No borrar bases de datos.

No ejecutar migraciones destructivas.

No utilizar:

npm audit fix --force

sin autorización.

No cambiar versiones mayores de Angular, .NET, pdfjs-dist u otras dependencias durante la conexión de API.

7. Calidad del código

Mantener TypeScript estricto.

Evitar any.

Se permite any únicamente cuando una librería externa lo haga necesario y no exista una alternativa razonable.

No duplicar DTOs innecesariamente.

No duplicar servicios.

Usar HttpClient para comunicación con API.

Usar Observable donde corresponda.

Usar Signals para estado local donde ya se estén utilizando.

No mezclar lógica HTTP directamente dentro de componentes cuando pueda estar en un service.

8. Enums

El backend utiliza enums y son la fuente de verdad.

Los enums se encuentran en:

Plandi.Dto.Enums

Los enums conocidos son:

EstadoPlaneacion
Borrador = 1
EnProceso = 2
EnRevision = 3
CorreccionSolicitada = 4
Aprobada = 5
Rechazada = 6
Generada = 7
TipoDocumento
ProgramaAsignatura = 1
PlantillaPlaneacion = 2
UsuariosCsv = 3
PlaneacionGenerada = 4
Anexo = 5
EstadoDocumento
Subido = 1
Procesando = 2
Procesado = 3
Error = 4
RolAcademia
Docente = 1
Revisor = 2
Director = 3
FaseSecuencia
Apertura = 1
Desarrollo = 2
Cierre = 3
TipoEvaluacion
Conceptual = 1
Producto = 2
Desempeno = 3
Ensayo = 4
EstudioDeCaso = 5
AnalisisDeDesempeno = 6
Proyecto = 7
Practica = 8
Reporte = 9
Exposicion = 10
Otro = 99
AgenteEvaluador
Autoevaluacion = 1
Coevaluacion = 2
Heteroevaluacion = 3

También existen:

EstrategiaApertura
EstrategiaDesarrollo
EstrategiaCierre

Consultar siempre el archivo real del backend antes de crear las equivalencias TypeScript.

No usar strings arbitrarios para representar enums que ya existen en backend.

Crear/mantener enums TypeScript equivalentes cuando el módulo real los necesite.

Los valores numéricos deben coincidir exactamente con backend.

9. Roles

No confundir:

RolAcademia

con los roles de autenticación JWT.

Los roles de autenticación deben obtenerse del backend/JWT.

El JWT actualmente agrega roles mediante:

new Claim(ClaimTypes.Role, ur.Rol.Nombre)

Por lo tanto, el frontend debe interpretar el claim real emitido por el backend.

No asumir que el claim se llama simplemente role.

ASP.NET puede serializar ClaimTypes.Role utilizando su URI de claim.

Inspeccionar el JWT real y el código existente antes de modificar el parser.

10. Autenticación

Backend utiliza JWT.

El token incluye al menos:

sub
email
jti
role/ClaimTypes.Role

según el código real de TokenService.

Existe:

accessToken
refreshToken

La sesión del frontend debe utilizar los tokens reales del backend.

El frontend debe poder:

iniciar sesión,
guardar sesión,
recuperar usuario desde JWT,
recuperar rol desde JWT,
cerrar sesión,
enviar Authorization Bearer,
manejar expiración,
utilizar refresh token cuando corresponda.

No volver a introducir usuarios mock.

El login mock anterior ya no debe ser utilizado.

11. Logout

Logout debe:

eliminar access token;
eliminar refresh token;
eliminar usuario almacenado;
limpiar estado reactivo;
redirigir a /auth/login.

No realizar llamadas HTTP inventadas para logout si backend no tiene endpoint de logout.

12. Recuperación de contraseña

El flujo elegido es:

Email
 ↓
Enlace de recuperación
 ↓
passwordResetToken
 ↓
Nueva contraseña

NO utilizar el flujo anterior de código de 6 dígitos.

NO implementar verify-code salvo que posteriormente el backend cambie.

Antes de modificarlo, inspeccionar los DTOs reales y AuthController.

El frontend tiene pantallas de:

recover-password
new-password

Estas deben conectarse con los endpoints reales del backend.

El token de recuperación debe obtenerse del enlace/query params según lo esperado por backend.

No hardcodear:

email
code
passwordResetToken

para producción.

13. CORS

Frontend de desarrollo:

http://localhost:4200

Backend observado durante desarrollo:

http://localhost:5198

El backend debe tener CORS correctamente configurado para desarrollo.

No utilizar * innecesariamente si ya existe una policy específica.

14. environment Angular

La URL de API debe centralizarse en environment.

Ejemplo actual esperado:

apiUrl: 'http://localhost:5198'

No hardcodear la URL del backend en cada service.

Usar:

environment.apiUrl
15. HttpClient

Angular utiliza configuración standalone.

provideHttpClient() debe estar configurado desde app.config.ts.

Si se agrega interceptor, utilizar la configuración compatible con la versión Angular instalada.

No crear un segundo HttpClient provider innecesariamente.

16. Interceptor JWT

Las peticiones protegidas deben enviar:

Authorization: Bearer ACCESS_TOKEN

No agregar Authorization a endpoints donde pueda causar problemas innecesarios si existe una estrategia de exclusión.

Considerar como públicos:

login
forgot-password
reset-password
refresh-token

según el comportamiento real del backend.

Si se implementa refresh automático:

evitar loops infinitos;
evitar refrescar el propio endpoint refresh-token;
cerrar sesión si refresh token es inválido;
no lanzar múltiples refresh simultáneos innecesariamente.
17. Guards

Los guards deben utilizar el estado real de AuthService.

No usar:

isAuthenticated

de localStorage como una bandera independiente si ya existe JWT/sesión real.

Los permisos por rol deben utilizar el rol real del usuario autenticado.

18. Sidebar

El sidebar depende del rol.

Actualmente existen visualmente:

DIRECTIVO
REVISOR
DOCENTE

No asumir que estos strings son exactamente los nombres almacenados en backend.

Debe existir una conversión explícita entre:

rol backend/JWT

y:

rol utilizado por UI

si los nombres son diferentes.

No otorgar DOCENTE por defecto cuando no existe rol válido.

Un rol desconocido debe tratarse como inválido/no autorizado, no como Docente.

19. Módulos principales

El frontend contiene módulos relacionados con:

Dashboard
Planeaciones
Validación
Reportes
Usuarios
Carreras
Asignaturas
Ciclos / Periodos
Academias
Grupos
Carga Académica
Asignación Académica
Importación
Seguimiento de Planeaciones

Antes de conectar cada uno buscar el Controller real correspondiente.

20. Servicios backend registrados

En Program.cs se han observado servicios como:

IUsuarioService
IAuthService
ITokenService
IEmailService
ICarreraService
IAsignaturaService
ICicloEscolarService
IPeriodoService
IGrupoService
IAcademiaService
ICargaAcademicaService
IPlaneacionCaratulaService
IPlaneacionTemaService
IPlaneacionEvaluacionService
IPlaneacionSecuenciaService
IPlaneacionReferenciaService

Esto NO significa automáticamente que exista un endpoint para cada método.

Siempre verificar Controllers.

21. Planeaciones

Planeaciones es uno de los módulos principales.

El frontend actualmente contiene modelos amplios y parte de ellos puede seguir siendo mock.

NO intentar hacer coincidir todo planeacion.model.ts con backend de una sola vez.

Primero identificar:

PlaneacionCaratula
PlaneacionTema
PlaneacionEvaluacion
PlaneacionSecuencia
PlaneacionReferencia

y sus respectivos DTOs/endpoints.

Conectar progresivamente.

22. Estados de planeación

El frontend originalmente utilizó strings como:

aprobado
borrador
revision
pendiente
correcciones

El backend tiene EstadoPlaneacion.

Para datos reales de API, utilizar el enum real del backend.

La UI puede utilizar funciones para obtener:

label,
clases CSS,
texto visible.

Ejemplo conceptual:

EstadoPlaneacion.Aprobada
        ↓
"Aprobado"
        ↓
clases visuales verdes

No almacenar el label visual como si fuera el estado real.

23. PDF.js

El proyecto ya tiene:

pdfjs-dist

instalado.

No sustituirlo por otra librería sin necesidad.

24. PDFs temporales

Actualmente se utilizan PDFs locales para probar la visualización:

/assets/pdf/Planeacion.pdf
/assets/pdf/Programa.pdf

Estos archivos son TEMPORALES.

No diseñar la integración definitiva alrededor de esas rutas.

25. Visores PDF

Existen:

PlaneacionPdfViewer
PlaneacionProgramaView

Ambos renderizan PDFs mediante PDF.js.

Ya existen funciones relacionadas con:

cargar PDF,
renderizar página,
página anterior,
página siguiente,
zoom,
imprimir,
descargar.

No rehacer estos componentes desde cero.

Cuando exista endpoint de PDF:

API
 ↓
URL o Blob
 ↓
visor existente

El objetivo es reemplazar únicamente la fuente del documento.

26. Worker PDF.js

El worker actualmente se sirve como:

/assets/pdf/pdf.worker.min.mjs

Angular tiene configuración para copiarlo desde:

node_modules/pdfjs-dist/build

No eliminar esta configuración mientras PDF.js la necesite.

27. Ciclo de vida de los visores PDF

Los visores pueden destruirse y crearse al cambiar pestañas.

NO llamar ciegamente:

this.pdfDocument.destroy();

porque la implementación actual produjo:

TypeError: this.pdfDocument.destroy is not a function

La implementación actual utiliza cancelación del render y referencias nulas.

Conservar las protecciones existentes relacionadas con:

destroyed
renderTask
requestAnimationFrame

salvo que exista una razón técnica comprobada para cambiarlas.

28. Programa PDF

El programa de asignatura actualmente usa:

/assets/pdf/Programa.pdf

solo para pruebas.

Posteriormente debe obtenerse desde backend.

No eliminar:

@Input({ required: true })
planeacion!: PlaneacionDetail;

de PlaneacionProgramaView, aunque temporalmente el PDF local no lo utilice.

Otros componentes dependen de ese Input.

29. Planeación PDF

La vista previa actualmente usa:

/assets/pdf/Planeacion.pdf

solo para pruebas.

Posteriormente deberá utilizar el documento generado/obtenido desde API.

30. Componentes compartidos

Antes de modificar un componente comprobar dónde se utiliza.

Especialmente:

PlaneacionPdfViewer
PlaneacionProgramaView
PlaneacionInfoPanel
PlaneacionForm

Algunos son utilizados tanto desde Planeaciones como desde Validación.

No eliminar Inputs sin buscar primero todas sus referencias.

31. Validación

El módulo Validación reutiliza componentes de Planeaciones.

Antes de modificar tipos utilizados por:

PlaneacionDetailPage
ValidacionDetail

comprobar ambos consumidores.

No romper Validación al conectar Planeaciones.

32. Datos mock

Muchos services frontend pueden contener:

of(...)

con datos simulados.

Antes de reemplazarlos:

identificar qué componentes consumen esos métodos;
encontrar endpoint real;
crear DTO correcto;
realizar HttpClient call;
adaptar response;
eliminar únicamente el mock reemplazado.

No eliminar todos los mocks globalmente de una sola vez.

33. DTOs frontend

Crear DTOs frontend que representen los contratos HTTP.

Preferir separación conceptual:

dto/
models/
services/

DTO:

estructura de API.

Model:

estructura utilizada por UI cuando sea necesario.

No forzar que DTO y modelo visual sean idénticos si tienen responsabilidades diferentes.

34. Manejo de errores

No usar únicamente:

console.log(error)

como manejo definitivo.

Los services deben propagar errores apropiadamente.

Los componentes deben mostrar mensajes donde la UI ya tenga soporte.

No sustituir silenciosamente un error HTTP por datos mock.

Si la API falla, debe quedar claro que falló.

35. Loading

Conservar estados de carga existentes.

Cuando un mock como:

of(...)

sea sustituido por HTTP real, verificar que:

loading
success
error

sigan funcionando correctamente.

36. Base de datos

No asumir datos existentes.

Si una pantalla queda vacía después de conectarla:

revisar response HTTP;
revisar endpoint;
revisar base de datos;
revisar mapping;
revisar filtros.

NO reintroducir mocks simplemente para que la pantalla tenga contenido.

37. Backend

No modificar backend solo porque el frontend sea más fácil de adaptar de otra forma.

Modificar backend únicamente cuando:

exista un bug real;
falte funcionalidad necesaria;
el contrato sea incorrecto;
exista autorización explícita.

Si puede resolverse correctamente adaptando frontend al contrato existente, preferir esa opción.

38. Seguridad

Nunca imprimir en logs:

contraseñas;
refresh tokens;
access tokens completos;
password reset tokens;
secretos JWT.

No guardar contraseñas en localStorage.

No hardcodear secretos.

No introducir credenciales reales en el repositorio.

39. JWT Secret

Nunca modificar o exponer:

Jwt:SecretKey

desde frontend.

El frontend nunca necesita conocer la clave de firma JWT.

40. Password reset token

El passwordResetToken debe considerarse sensible.

No imprimirlo innecesariamente en consola.

No persistirlo permanentemente en localStorage.

Usarlo únicamente durante el flujo de recuperación.

41. Comandos frontend

Antes de ejecutar comandos, verificar scripts disponibles en package.json.

Comandos esperados:

cd frontend
npm install
npm start

Para validar compilación usar el script existente o:

ng build

No asumir un comando si package.json define otro.

42. Comandos backend

Desde la ubicación correspondiente:

dotnet build .\Plandi.API\Plandi.API.csproj

Para ejecutar:

dotnet run --project .\Plandi.API\Plandi.API.csproj

La API se ha observado en desarrollo en:

http://localhost:5198

Confirmar siempre launchSettings.json.

43. Verificación después de cambios

Después de modificar frontend:

compilar

y revisar errores TypeScript/Angular.

Después de modificar backend:

dotnet build

No declarar una tarea terminada si la compilación falla debido a cambios realizados.

44. Errores existentes

Si se encuentran errores que ya existían antes del cambio:

reportarlos;
distinguirlos de errores introducidos;
no arreglar módulos no relacionados automáticamente.
45. Cambios mínimos

Preferir cambios pequeños.

Ejemplo:

Incorrecto:

Conectar Carrera
+
reescribir AuthService
+
cambiar routing
+
refactorizar Planeaciones
+
actualizar Tailwind

Correcto:

Conectar Carrera
+
DTO necesario
+
CarreraService
+
componentes consumidores
+
build
46. No sobreingeniería

No crear:

repositories frontend innecesarios;
stores globales innecesarios;
capas genéricas HTTP innecesarias;
abstracciones complejas para CRUD simple.

Mantener la arquitectura existente mientras sea razonable.

47. Plan obligatorio antes de cambios grandes

Si una tarea requiere modificar más de aproximadamente 5 archivos relacionados:

Primero:

inspeccionar;
enumerar archivos;
explicar el cambio;
después implementar.

No hacer cambios masivos sin inspección previa.

48. Primera fase obligatoria

Antes de continuar conectando módulos, realizar un inventario del backend.

Buscar todos los Controllers.

Construir un mapa:

Frontend
        Backend
---------------------------------------
Auth -> AuthController
Usuarios -> ?
Carreras -> ?
Asignaturas -> ?
Ciclos -> ?
Periodos -> ?
Academias -> ?
Grupos -> ?
Carga Académica -> ?
Planeaciones -> ?
Validación -> ?
Reportes -> ?
PDF -> ?

Para cada uno registrar:

HTTP method
endpoint
request DTO
response DTO
auth requerida
roles/policies
service backend
service frontend actual
usa mock actualmente
49. Orden de integración

Después del inventario trabajar en este orden:

Fase 1
Auth

Incluye:

login;
sesión;
JWT;
roles;
logout;
forgot password;
reset password;
refresh token;
interceptor;
guards.
Fase 2
Usuarios
Carreras
Asignaturas
Ciclos
Periodos
Academias
Grupos
Fase 3
Carga académica
Asignación académica
Importaciones
Fase 4
Planeaciones
Fase 5
Validación
Fase 6
PDFs reales
Fase 7
Reportes
Seguimiento
50. Primera tarea para Codex

Al recibir este repositorio por primera vez:

NO modificar código todavía.

Primero:

Leer este AGENTS.md.
Inspeccionar estructura completa del frontend.
Inspeccionar estructura completa del backend.
Buscar todos los Controllers.
Buscar todos los DTOs utilizados por esos Controllers.
Buscar enums.
Buscar services backend.
Buscar services Angular.
Buscar todos los usos de datos mock (of(...), arrays hardcodeados, setTimeout simulando API, etc.).
Construir el mapa Frontend -> Backend.
Identificar endpoints que no tengan una pantalla frontend.
Identificar pantallas frontend para las que no exista endpoint.
Identificar incompatibilidades entre DTOs frontend y backend.
Identificar strings frontend que deberían utilizar enums.
Identificar funcionalidades todavía simuladas.

Después presentar:

inventario;
incompatibilidades;
riesgos;
orden recomendado de cambios.

NO comenzar cambios masivos hasta terminar este análisis.

51. Regla final

Preservar todo lo que ya funciona.

La prioridad es:

Contrato backend correcto
        +
Frontend existente
        +
Cambios mínimos
        +
Compilación exitosa
        +
Integración progresiva

No perseguir una arquitectura "más bonita" a costa de romper funcionalidad existente.