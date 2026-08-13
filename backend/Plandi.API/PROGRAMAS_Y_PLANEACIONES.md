# Programas de asignatura y planeaciones

## Importación de programas

`POST /api/programas-asignatura/importar` recibe `multipart/form-data` con `files` (uno o más PDF) y `subidoPorPublicId` (GUID de un usuario existente). Cada PDF se procesa de forma independiente. La importación extrae el texto, lo guarda en `programas_asignatura.texto_extraido` y persiste el JSON estructurado en `json_extraido`; nunca crea una planeación.

El PDF debe contener al menos el nombre y clave de la asignatura. Se guarda un `Documento`, se relaciona con `ProgramaAsignatura`, se crea la asignatura sólo si no existe y se actualiza el programa cuando coincide la combinación asignatura y cuatrimestre. El mismo archivo se identifica por SHA-256 y se devuelve como ya importado.

Ejemplo Swagger/formulario:

```text
files=@35_82_Q3_BASES DE DATOS_2024.pdf
subidoPorPublicId=<GUID de usuario>
```

## Generación explícita de planeaciones

`POST /api/Planeaciones/generar` no recibe archivos. Consulta programas importados y sus cargas académicas activas. Para cada combinación de período y asignatura crea una única `PlaneacionDidactica`, su carátula y las unidades extraídas del JSON. Si ya existe la planeación para ese período y asignatura, se informa como existente.

Los docentes y grupos se obtienen de `carga_academica`; se agregan en los campos de texto de la carátula porque el modelo actual de `PlaneacionDidactica` no tiene FK directa a grupo o docente. Cuando no existe una carga académica, el programa se omite con mensaje y no se inventan relaciones.

El JSON conserva el contenido fuente de cada unidad. En esta primera extracción no se crean temas, evaluaciones ni secuencias cuando el PDF no permite separar de manera confiable sus columnas; esos datos quedan disponibles para revisión en el JSON y pueden completarse desde los endpoints existentes de planeaciones.
