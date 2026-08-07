# Integradora

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.12.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
# Sistema Académico - Planeación Didáctica
Frontend desarrollado con Angular Standalone + TailwindCSS para la gestión digital de Planeaciones Didácticas.

---

# Tecnologías utilizadas

- Angular 20 (Standalone Components)
- TypeScript
- TailwindCSS
- Lucide Angular (Iconografía)
- RxJS
- Signals (Angular)
- Angular Router
- HTML + CSS

---

# Arquitectura

El proyecto sigue una arquitectura modular basada en funcionalidades (Feature-Based Architecture).

Cada módulo contiene únicamente los componentes relacionados con una funcionalidad del sistema.

```
src/
│
├── app/
│   ├── auth/
│   ├── core/
│   ├── enums/
│   ├── environments/
│   ├── features/
│   ├── layout/
│   ├── app.routes.ts
│   ├── app.config.ts
│   └── ...
```

---

# Estructura del proyecto

## app/auth

Contiene la autenticación básica del sistema.

Actualmente incluye:

- Login
- Recuperación de contraseña
- Manejo del usuario autenticado

Su objetivo es únicamente controlar el acceso al sistema.

---

## app/core

Es el núcleo de la aplicación.

Aquí se concentra toda la lógica reutilizable.

Contiene principalmente:

### models

Interfaces utilizadas por toda la aplicación.

Ejemplos:

- Planeacion
- Programa
- Unidad
- Actividades
- Referencias
- Usuarios
- Validaciones

Estas interfaces representan exactamente la información que posteriormente recibirá el Backend.

---

### services

Servicios que actualmente funcionan con datos simulados (Mock).

Ejemplos:

- auth.service
- planeaciones.service
- validacion.service

Posteriormente estos servicios consumirán directamente la API.

---

### guards

Protección de rutas.

Actualmente existe:

- authGuard

Se encarga de impedir el acceso a usuarios no autenticados.

---

### interceptors (futuro)

Aquí se colocarán:

- JWT Token
- Manejo de errores
- Refresh Token

---

### helpers

Funciones reutilizables.

---

### interfaces

Interfaces generales utilizadas por varios módulos.

---

# app/enums

Contiene enumeraciones globales.

Ejemplos:

- Roles
- Estados
- Tipos
- Constantes

---

# environments

Configuración de ambientes.

Actualmente se utilizará para definir:

```
API_URL
```

por ejemplo

```
http://localhost:5000/api
```

o

```
https://api.uth.edu.mx
```

---

# features

Es el corazón del sistema.

Cada carpeta representa un módulo funcional.

---

# features/auth

Contiene toda la interfaz de autenticación.

Incluye:

- Login
- Recuperación de contraseña

---

# features/dashboard

Pantalla principal del sistema.

Dependiendo del rol muestra información distinta.

Actualmente existen dashboards para:

- Docente
- Revisor
- Administrador / Directivo

Incluye:

- Estadísticas
- Acciones rápidas
- Actividad reciente

---

# features/planeaciones

Es el módulo más grande del proyecto.

Aquí se realiza toda la captura de la planeación.

Contiene:

## Lista de planeaciones

Muestra todas las planeaciones del docente.

Permite:

- Crear
- Editar
- Consultar
- Enviar a revisión

---

## Detalle de planeación

Pantalla principal de captura.

Actualmente contiene:

### Carátula

Información general.

### Unidades

Cada unidad contiene:

- Resultado de aprendizaje
- Saberes
- Evaluación
- Apertura
- Desarrollo
- Cierre
- Referencias

---

### Panel lateral

Incluye:

- Información
- Guía de llenado
- Acciones

El panel permanece visible mientras el usuario navega.

---

### Chat docente - revisor

Comunicación entre:

- Docente
- Revisor

No existe comunicación entre docentes.

---

### Viewer PDF

Componente encargado de mostrar el PDF de la planeación.

Actualmente funciona con Mock.

Posteriormente recibirá una URL enviada por la API.

---

### Programa

Vista del programa de asignatura.

---

# features/validacion

Módulo exclusivo del Revisor.

Contiene:

## Lista de validaciones

Muestra todas las planeaciones pendientes.

---

## Detalle

Permite:

Visualizar

- PDF

Programa

Planeación

Chat

Acciones

- Aprobar
- Solicitar correcciones

Al abrir una planeación pendiente automáticamente cambia a:

```
En revisión
```

---

# features/reportes

Módulo destinado a reportes.

Actualmente contiene la estructura inicial.

Posteriormente permitirá generar:

- PDF
- Excel
- Indicadores

---

# features/admin

Módulo del Administrador / Directivo.

Actualmente concentra toda la administración.

Incluye:

---

## Usuarios

CRUD de usuarios.

---

## Carreras

Administración de carreras.

---

## Asignaturas

Administración de asignaturas.

---

## Ciclos y periodos

Administración de periodos escolares.

---

## Academias

Gestión de academias.

---

## Grupos

Administración de grupos.

---

## Asignación académica

Asignación de docentes.

---

## Importación

Importación masiva de:

- Academias
- Profesores

---

## Seguimiento de planeaciones

Vista Directiva.

Permitirá visualizar:

- Fecha de creación
- Fecha de envío
- Fecha de revisión
- Fecha de aprobación
- Estado actual
- Revisor asignado

---

# layout

Contiene toda la estructura visual del sistema.

---

## Main Layout

Layout principal.

Incluye:

- Sidebar
- Área de contenido

---

## Sidebar

Menú lateral.

Se adapta automáticamente al rol.

Actualmente existen tres vistas:

Docente

Revisor

Administrador / Directivo

El menú es completamente dinámico.

---

# Flujo actual

Login

↓

Dashboard

↓

Planeaciones

↓

Captura

↓

Enviar a revisión

↓

Revisor

↓

Correcciones o aprobación

↓

Directivo

↓

Seguimiento

---

# Estado actual del proyecto

Actualmente ya se encuentra implementado:

✔ Login

✔ Dashboard

✔ Sidebar dinámica

✔ Planeaciones

✔ Captura completa

✔ Panel lateral

✔ Chat docente - revisor

✔ Validaciones

✔ Cambios automáticos de estado

✔ Reportes base

✔ Administración

✔ Seguimiento Directivo (estructura)

✔ Interfaces completas

✔ Servicios Mock

---

# Pendiente

## Integración Backend

Se sustituirán los servicios Mock por llamadas HTTP.

Ejemplo:

```
PlaneacionesService

ANTES

of(data)

DESPUÉS

HttpClient.get(...)
```

---

## JWT

Integración con autenticación real.

---

## Notificaciones

Cambio de estado.

Correo.

Avisos.

---

## Visualización PDF

Mostrar el PDF generado por el Backend.

---

## Exportación

Word

PDF

Excel

---

# Convenciones del proyecto

- Arquitectura Feature-Based.
- Componentes Standalone.
- Signals para estado local.
- Interfaces centralizadas en Core.
- Servicios desacoplados.
- TailwindCSS como sistema principal de estilos.
- CSS únicamente para animaciones o estilos muy específicos.

---

# Próximo paso

El siguiente objetivo del proyecto será reemplazar completamente los datos Mock por consumo real de la API ASP.NET Core, manteniendo las interfaces ya definidas para minimizar cambios en los componentes del frontend.

