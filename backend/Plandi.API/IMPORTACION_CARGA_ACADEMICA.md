# Importación de carga académica

`POST /api/CargasAcademicas/importar` recibe `multipart/form-data`:

- `file`: archivo `.csv` (coma o punto y coma) o `.xlsx`.
- `periodoPublicId`: GUID de un período activo ya registrado.

El encabezado debe incluir `Asignatura`, `Cuatrimestre`, `P.E.` (también se acepta `PE` o `Programa Educativo`) y `Docente`. Puede estar después de filas de título, como ocurre en `Academias.xlsx`; la columna `Hrs.` se admite, pero no se procesa porque la solicitud indica ignorarla.

`Cuatrimestre` se interpreta como número y letra: `3A` crea o reutiliza el grupo `3A`, con `Cuatrimestre = 3` y carrera igual al P.E. La relación con el P.E. se guarda en `Grupo.CarreraId`, no en el nombre del grupo. El período no puede inferirse del archivo, por eso es obligatorio enviarlo en el formulario.

El P.E. se relaciona con `Carrera` por `Clave`; si no existe se crea con el mismo valor como clave y nombre. Las asignaturas se comparan por nombre normalizado (sin acentos, espacios repetidos ni diferencias de mayúsculas). Si no existe una, se crea con una clave técnica determinista `IMP-<cuatrimestre>-<hash>` ya que el archivo no contiene la clave requerida por el modelo. Sus horas y créditos se guardan en cero.

Los docentes se comparan contra el nombre completo de `Usuario` con la misma normalización. Los prefijos académicos (`Mtro.`, `Mtra.`, `Ing.`, `Lic.`, etc.) se descartan y el último par de palabras se guarda como apellido paterno y materno; las palabras anteriores son el nombre. Si no existe, se crea un usuario sin correo ni contraseña y se le asigna el rol existente `Docente`, ya que esas columnas permiten valores nulos para importaciones.

La relación final se guarda en `CargaAcademica` con período, grupo, asignatura y docente. La combinación ya tiene un índice único en la base; además se detectan duplicados dentro del archivo para que repetir una importación no cree nuevas cargas.

Ejemplo:

```http
POST /api/CargasAcademicas/importar
Content-Type: multipart/form-data

file=@Academias.xlsx
periodoPublicId=60000000-0000-0000-0000-000000000001
```

La respuesta usa el envoltorio estándar de la API. `data.errores` contiene `fila`, `campo`, `valor` y `mensaje`; las filas válidas se confirman incluso si otras filas presentan errores. Un error técnico inesperado revierte toda la transacción.
