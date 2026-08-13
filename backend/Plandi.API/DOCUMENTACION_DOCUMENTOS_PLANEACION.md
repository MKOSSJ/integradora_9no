# Documentos de planeación — API

Todos los endpoints de este documento usan `Authorization: Bearer <token>`. Los identificadores expuestos son `PublicId` (GUID), nunca IDs internos ni rutas físicas.

| Método | Endpoint | Función | Respuesta |
| --- | --- | --- | --- |
| GET | `/api/planeaciones/{id}` | Detalle y enlaces de recursos | JSON |
| GET | `/api/programas-asignatura/{id}/archivo` | Visualiza el programa | PDF |
| GET | `/api/programas-asignatura/{id}/archivo/descarga` | Descarga el programa | PDF adjunto |
| GET | `/api/planeaciones/{id}/pdf` | Genera y visualiza el PDF actual | PDF |
| GET | `/api/planeaciones/{id}/pdf/descarga` | Genera y descarga el PDF actual | PDF adjunto |
| POST | `/api/plantillas/planeacion` | Carga y activa una plantilla | JSON, 201 |
| GET | `/api/plantillas/planeacion/activa` | Metadata de la plantilla activa | JSON |
| GET | `/api/plantillas/planeacion/{id}/archivo` | Visualiza la plantilla DOCX | DOCX |
| GET | `/api/plantillas/planeacion/{id}/archivo/descarga` | Descarga la plantilla DOCX | DOCX adjunto |

## Permisos

Para recursos de una planeación se verifica el permiso efectivo: un docente debe tener la carga académica correspondiente, un revisor debe estar asignado a la planeación y Director tiene acceso global. Los roles se consultan en la relación usuario-rol, por lo que una cuenta con Docente y Revisor conserva ambos permisos.

La administración de plantillas requiere Director. Una petición sin token devuelve `401`; un token válido sin permiso devuelve `403`.

## Detalle

`GET /api/planeaciones/{planeacionPublicId}` acepta `Accept: application/json` y devuelve el detalle editable que ya usa el flujo, más enlaces de archivos.

```json
{
  "success": true,
  "data": {
    "planeacion": { "publicId": "...", "estado": 1, "caratula": {}, "unidades": [], "referencias": [] },
    "archivos": {
      "programaAsignatura": {
        "disponible": true,
        "nombre": "Programa_Programacion.pdf",
        "mimeType": "application/pdf",
        "urlVisualizacion": "/api/programas-asignatura/{id}/archivo",
        "urlDescarga": "/api/programas-asignatura/{id}/archivo/descarga"
      },
      "planeacionDidactica": {
        "disponible": true,
        "nombre": "Planeacion_Programacion.pdf",
        "mimeType": "application/pdf",
        "urlVisualizacion": "/api/planeaciones/{id}/pdf",
        "urlDescarga": "/api/planeaciones/{id}/pdf/descarga"
      }
    }
  }
}
```

`programaAsignatura.disponible` es `false` si la planeación no tiene programa o si su archivo ya no está disponible. El PDF didáctico se genera bajo demanda y nunca se conserva como fuente de verdad.

## Archivos PDF

Los cuatro endpoints de programa y PDF reciben `Accept: application/pdf` (opcional) y devuelven bytes; no devuelven un PDF codificado dentro de JSON.

La variante de visualización responde `200`, `Content-Type: application/pdf` y `Content-Disposition: inline` (comportamiento por defecto de ASP.NET). La variante `/descarga` agrega `Content-Disposition: attachment; filename="...pdf"`.

Ejemplo cURL:

```bash
curl -L -H "Authorization: Bearer TOKEN" -H "Accept: application/pdf" \
  -o Planeacion.pdf "https://servidor/api/planeaciones/{id}/pdf/descarga"
```

## Plantillas

`POST /api/plantillas/planeacion` usa `multipart/form-data` con el campo obligatorio `file`. Acepta solamente `.docx`, MIME DOCX u `application/octet-stream`, hasta 25 MB, y valida que sea un ZIP DOCX con las partes requeridas. La nueva carga se activa dentro de una transacción y desactiva la anterior, conservando el historial.

```bash
curl -X POST -H "Authorization: Bearer TOKEN" \
  -F "file=@plantilla_secuencia.docx;type=application/vnd.openxmlformats-officedocument.wordprocessingml.document" \
  https://servidor/api/plantillas/planeacion
```

Respuesta `201 Created`:

```json
{"success":true,"data":{"id":"GUID","nombre":"plantilla_secuencia.docx","version":1,"activa":true,"fechaCarga":"2026-08-13T17:00:00Z"},"message":"Plantilla cargada y activada."}
```

La plantilla admite sustituciones `{{PROGRAMA_EDUCATIVO}}`, `{{CUATRIMESTRE}}`, `{{ASIGNATURA}}`, `{{DOCENTES}}`, `{{PERIODO}}`, `{{GRUPOS}}`, `{{PROPOSITO}}`, `{{COMPETENCIA}}`, `{{CREDITOS}}`, `{{MODALIDAD}}`, `{{HORAS_SABER}}`, `{{HORAS_SABER_HACER}}`, `{{HORAS_TOTALES}}` y `{{HORAS_SEMANA}}`. El servicio trabaja sobre una copia temporal, agrega secciones B/C/D por cada unidad existente y elimina los marcadores `1)` a `35)` del documento generado.

## Errores

Los errores siguen el envelope existente:

```json
{"success":false,"message":"La planeación solicitada no existe.","errors":null}
```

Los códigos son `400` para archivos/formularios inválidos, `401` sin autenticación, `403` sin permiso efectivo, `404` para recursos inexistentes y `500` cuando la conversión real a PDF falla. No se exponen rutas ni trazas.

## Conversión real DOCX a PDF

El servicio utiliza LibreOffice en modo headless, sin Microsoft Word. Instale LibreOffice en el servidor y configure la ruta ejecutable:

```json
"PdfConversion": { "LibreOfficePath": "C:\\Program Files\\LibreOffice\\program\\soffice.exe" }
```

En Linux puede utilizar `"soffice"` si está en `PATH`. La conversión ejecuta `soffice --headless --convert-to pdf:writer_pdf_Export`. Si falta LibreOffice o no genera el PDF, la API responde `500` con un mensaje seguro.

## Consumo posterior desde Angular

No se modificó Angular. Ejemplos conceptuales:

```typescript
this.http.get(`${apiUrl}/api/planeaciones/${id}`);
this.http.get(`${apiUrl}/api/planeaciones/${id}/pdf`, { responseType: 'blob' });
this.http.get(`${apiUrl}/api/planeaciones/${id}/pdf/descarga`, { responseType: 'blob' });
this.http.get(`${apiUrl}/api/programas-asignatura/${programaId}/archivo`, { responseType: 'blob' });
```
