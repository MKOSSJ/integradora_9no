using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class ImportacionCargaAcademicaService : IImportacionCargaAcademicaService
{
    public const long MaxFileBytes = 10 * 1024 * 1024;
    private const int MaxRows = 5000;
    private const int MaxColumns = 50;
    private const int MaxCellLength = 500;
    private static readonly TimeSpan ProcessingTimeout = TimeSpan.FromMinutes(2);
    private readonly AppDbContext _dbContext;
    private readonly IPeriodoLifecycleService _lifecycle;

    public ImportacionCargaAcademicaService(AppDbContext dbContext, IPeriodoLifecycleService lifecycle)
    {
        _dbContext = dbContext;
        _lifecycle = lifecycle;
    }
    public ImportacionCargaAcademicaService(AppDbContext dbContext) : this(dbContext, PeriodoLifecycleService.ForContext(dbContext)) { }

    public async Task<ImportacionCargaAcademicaResultadoDto> Importar(
        Stream archivo, string nombreArchivo, Guid periodoPublicId, long importadoPorId, CancellationToken cancellationToken = default)
    {
        if (archivo.CanSeek && archivo.Length > MaxFileBytes)
            throw new AppException("El archivo de carga académica no puede exceder 10 MB.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ProcessingTimeout);
        cancellationToken = timeout.Token;
        var periodo = await _dbContext.Periodos.SingleOrDefaultAsync(p => p.PublicId == periodoPublicId && p.Activo && p.DeletedAt == null, cancellationToken)
            ?? throw new AppException("El periodo especificado no existe o no está activo.");
        await _lifecycle.ExigirEditableAsync(periodo.Id, cancellationToken);

        var filas = await LeerFilas(archivo, nombreArchivo, cancellationToken);
        var resultado = new ImportacionCargaAcademicaResultadoDto { TotalFilas = filas.Count };
        var relacionesEnArchivo = new HashSet<string>(StringComparer.Ordinal);

        await using var transaccion = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var fila in filas)
            {
                if (!ValidarFila(fila, resultado))
                    continue;

                var grupo = ParsearGrupo(fila.Cuatrimestre!);
                if (grupo is null)
                {
                    AgregarError(resultado, fila, "Cuatrimestre", fila.Cuatrimestre, "El valor debe tener el formato número seguido de grupo, por ejemplo 3A.");
                    continue;
                }

                var docente = await BuscarOCrearDocente(fila.Docente!, cancellationToken);
                if (docente is null)
                {
                    AgregarError(resultado, fila, "Docente", fila.Docente, "No fue posible separar el nombre y apellidos del docente.");
                    continue;
                }

                var carrera = await BuscarOCrearCarrera(fila.ProgramaEducativo!, importadoPorId, cancellationToken);
                var entidadGrupo = await BuscarOCrearGrupo(periodo, carrera, grupo, importadoPorId, cancellationToken);
                var asignatura = await BuscarOCrearAsignatura(fila.Asignatura!, grupo.Cuatrimestre, importadoPorId, cancellationToken);
                // Se persisten primero los catálogos para disponer de sus FK y poder comprobar la relación compuesta.
                await _dbContext.SaveChangesAsync(cancellationToken);
                var claveRelacion = $"{periodo.Id}|{entidadGrupo.Id}|{asignatura.Id}|{docente.Id}";

                if (!relacionesEnArchivo.Add(claveRelacion) || await _dbContext.CargasAcademicas.AnyAsync(c =>
                        c.PeriodoId == periodo.Id && c.GrupoId == entidadGrupo.Id && c.AsignaturaId == asignatura.Id &&
                        c.DocenteId == docente.Id && c.Activo && c.DeletedAt == null, cancellationToken))
                {
                    resultado.Omitidas++;
                    continue;
                }

                _dbContext.CargasAcademicas.Add(new CargaAcademica
                {
                    PeriodoId = periodo.Id,
                    GrupoId = entidadGrupo.Id,
                    AsignaturaId = asignatura.Id,
                    DocenteId = docente.Id,
                    CreatedBy = importadoPorId
                });
                resultado.Insertadas++;
                resultado.Procesadas++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return resultado;
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Usuario?> BuscarOCrearDocente(string nombre, CancellationToken cancellationToken)
    {
        var partes = SepararNombreDocente(nombre);
        if (partes is null) return null;
        ValidarLongitud(partes.Nombre, 100, "El nombre del docente no puede exceder 100 caracteres.");
        ValidarLongitud(partes.ApellidoPaterno, 100, "El apellido paterno del docente no puede exceder 100 caracteres.");
        ValidarLongitud(partes.ApellidoMaterno, 100, "El apellido materno del docente no puede exceder 100 caracteres.");

        var candidatos = (await _dbContext.Usuarios.Where(u => u.Activo && u.DeletedAt == null).ToListAsync(cancellationToken))
            .Concat(_dbContext.Usuarios.Local.Where(u => u.Activo && u.DeletedAt == null))
            .Distinct()
            .ToList();
        var buscado = Normalizar(partes.NombreCompleto);
        var existente = candidatos.SingleOrDefault(u => Normalizar($"{u.Nombre} {u.ApellidoPaterno} {u.ApellidoMaterno}") == buscado);
        if (existente is not null) return existente;

        var docente = new Usuario
        {
            Nombre = partes.Nombre,
            ApellidoPaterno = partes.ApellidoPaterno,
            ApellidoMaterno = partes.ApellidoMaterno,
            Email = null,
            PasswordHash = null
        };
        _dbContext.Usuarios.Add(docente);

        var rolDocente = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Activo && r.DeletedAt == null && r.Nombre == "Docente", cancellationToken);
        if (rolDocente is not null)
        {
            _dbContext.UsuarioRoles.Add(new UsuarioRol { Usuario = docente, RolId = rolDocente.Id });
        }
        return docente;
    }

    private async Task<Carrera> BuscarOCrearCarrera(string clave, long actorId, CancellationToken cancellationToken)
    {
        var valor = clave.Trim();
        ValidarLongitud(valor, 50, "La clave de la carrera no puede exceder 50 caracteres.");
        ValidarLongitud(valor, 200, "El nombre de la carrera no puede exceder 200 caracteres.");
        var normalizada = Normalizar(clave);
        var existente = await _dbContext.Carreras.FirstOrDefaultAsync(c => c.Activo && c.DeletedAt == null && c.Clave.ToUpper() == normalizada, cancellationToken);
        if (existente is not null) return existente;

        var carrera = new Carrera { Clave = valor, Nombre = valor, CreatedBy = actorId };
        _dbContext.Carreras.Add(carrera);
        return carrera;
    }

    private async Task<Grupo> BuscarOCrearGrupo(Periodo periodo, Carrera carrera, GrupoImportado grupo, long actorId, CancellationToken cancellationToken)
    {
        // La carrera se relaciona directamente por Grupo.CarreraId; el nombre sólo representa cuatrimestre y letra (p. ej. 3A).
        var nombre = grupo.Codigo;
        ValidarLongitud(nombre, 50, "El nombre del grupo no puede exceder 50 caracteres.");
        var existente = await _dbContext.Grupos.FirstOrDefaultAsync(g => g.Activo && g.DeletedAt == null && g.PeriodoId == periodo.Id && g.Nombre.ToUpper() == nombre.ToUpper(), cancellationToken);
        if (existente is not null) return existente;

        var entidad = new Grupo { Nombre = nombre, Cuatrimestre = grupo.Cuatrimestre, Carrera = carrera, PeriodoId = periodo.Id, CreatedBy = actorId };
        _dbContext.Grupos.Add(entidad);
        return entidad;
    }

    private async Task<Asignatura> BuscarOCrearAsignatura(string nombre, int cuatrimestre, long actorId, CancellationToken cancellationToken)
    {
        var normalizado = Normalizar(nombre);
        var candidatas = await _dbContext.Asignaturas.Where(a => a.Activo && a.DeletedAt == null).ToListAsync(cancellationToken);
        var existente = candidatas.FirstOrDefault(a => Normalizar(a.Nombre) == normalizado);
        if (existente is not null) return existente;

        // La entidad requiere una clave, que el archivo no proporciona. Se deriva de manera determinista del nombre para conservar idempotencia.
        var asignatura = new Asignatura
        {
            Nombre = Limitar(nombre.Trim(), 200),
            Clave = $"IMP-{cuatrimestre}-{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(normalizado)))[..12]}",
            Cuatrimestre = cuatrimestre,
            HorasTotales = 0,
            HorasSemana = 0,
            Creditos = 0,
            CreatedBy = actorId
        };
        _dbContext.Asignaturas.Add(asignatura);
        return asignatura;
    }

    private static bool ValidarFila(FilaImportada fila, ImportacionCargaAcademicaResultadoDto resultado)
    {
        var valida = true;
        foreach (var (campo, valor) in new[] { ("Asignatura", fila.Asignatura), ("Cuatrimestre", fila.Cuatrimestre), ("P.E.", fila.ProgramaEducativo), ("Docente", fila.Docente) })
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                AgregarError(resultado, fila, campo, valor, "El campo es obligatorio.");
                valida = false;
            }
        }
        return valida;
    }

    private static void AgregarError(ImportacionCargaAcademicaResultadoDto resultado, FilaImportada fila, string campo, string? valor, string mensaje)
    {
        resultado.Omitidas++;
        resultado.Errores.Add(new ImportacionCargaAcademicaErrorDto { Fila = fila.Numero, Campo = campo, Valor = valor, Mensaje = mensaje });
    }

    private static GrupoImportado? ParsearGrupo(string valor)
    {
        var match = Regex.Match(valor.Trim(), "^(?<cuatrimestre>[1-9][0-9]*)\\s*(?<grupo>[A-Za-z]+)$");
        return !match.Success ? null : new GrupoImportado(int.Parse(match.Groups["cuatrimestre"].Value, CultureInfo.InvariantCulture), match.Groups["grupo"].Value.ToUpperInvariant());
    }

    private static string Normalizar(string? valor)
    {
        var texto = Regex.Replace((valor ?? string.Empty).Trim(), "\\s+", " ").ToUpperInvariant().Normalize(NormalizationForm.FormD);
        return new string(texto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
    }

    private static string Limitar(string valor, int longitud) => valor.Length <= longitud ? valor : valor[..longitud];

    private static void ValidarLongitud(string valor, int longitud, string mensaje)
    {
        if (valor.Length > longitud) throw new AppException(mensaje);
    }

    private static DatosDocente? SepararNombreDocente(string valor)
    {
        var sinPrefijos = Regex.Replace(valor.Trim(), @"^(?:(?:Mtro|Mtra|Mt\.?ra|Ing|Lic|Ma|Dr|Dra|Prof|Profa|C\.?P\.?|M\.? en C\.?)\.?\s+)+", string.Empty, RegexOptions.IgnoreCase);
        var palabras = Regex.Replace(sinPrefijos, "\\s+", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (palabras.Length < 3) return null;

        var apellidoMaterno = palabras[^1];
        var apellidoPaterno = palabras[^2];
        var nombre = string.Join(' ', palabras[..^2]);
        return new DatosDocente(nombre, apellidoPaterno, apellidoMaterno);
    }

    private static async Task<List<FilaImportada>> LeerFilas(Stream archivo, string nombreArchivo, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        List<List<string>> tabla = extension switch
        {
            ".csv" => await LeerCsv(archivo, cancellationToken),
            ".xlsx" => LeerXlsx(archivo),
            _ => throw new AppException("Sólo se aceptan archivos .csv o .xlsx.")
        };
        ValidarLimites(tabla);
        return ConvertirTabla(tabla);
    }

    private static async Task<List<List<string>>> LeerCsv(Stream archivo, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(archivo, Encoding.UTF8, true, leaveOpen: true);
        var contenido = await reader.ReadToEndAsync(cancellationToken);
        var delimitador = contenido.Count(c => c == ';') > contenido.Count(c => c == ',') ? ';' : ',';
        return contenido.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => ParsearCsv(x, delimitador)).ToList();
    }

    private static List<string> ParsearCsv(string linea, char delimitador)
    {
        var valores = new List<string>(); var actual = new StringBuilder(); var entreComillas = false;
        for (var i = 0; i < linea.Length; i++)
        {
            if (linea[i] == '"' && entreComillas && i + 1 < linea.Length && linea[i + 1] == '"') { actual.Append('"'); i++; }
            else if (linea[i] == '"') entreComillas = !entreComillas;
            else if (linea[i] == delimitador && !entreComillas) { valores.Add(actual.ToString()); actual.Clear(); }
            else actual.Append(linea[i]);
        }
        valores.Add(actual.ToString()); return valores;
    }

    private static List<List<string>> LeerXlsx(Stream archivo)
    {
        using var zip = new ZipArchive(archivo, ZipArchiveMode.Read, leaveOpen: true);
        if (zip.Entries.Sum(entry => entry.Length) > MaxFileBytes * 5)
            throw new AppException("El contenido descomprimido del XLSX excede el límite permitido.");
        var sharedStrings = zip.GetEntry("xl/sharedStrings.xml") is { } shared ? XDocument.Load(shared.Open()).Descendants().Where(x => x.Name.LocalName == "si").Select(x => string.Concat(x.Descendants().Where(n => n.Name.LocalName == "t").Select(n => n.Value))).ToList() : [];
        var hoja = zip.Entries.FirstOrDefault(e => e.FullName.Equals("xl/worksheets/sheet1.xml", StringComparison.OrdinalIgnoreCase)) ?? zip.Entries.FirstOrDefault(e => e.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase));
        if (hoja is null) throw new AppException("El libro no contiene una hoja de trabajo.");
        var documento = XDocument.Load(hoja.Open());
        return documento.Descendants().Where(x => x.Name.LocalName == "row").Select(fila =>
        {
            var celdas = new SortedDictionary<int, string>();
            foreach (var celda in fila.Descendants().Where(x => x.Name.LocalName == "c"))
            {
                var tipo = (string?)celda.Attribute("t");
                var valor = celda.Descendants().FirstOrDefault(x => x.Name.LocalName == "v")?.Value ?? string.Concat(celda.Descendants().Where(x => x.Name.LocalName == "t").Select(x => x.Value));
                var referencia = (string?)celda.Attribute("r") ?? "A1";
                var columna = 0;
                foreach (var letra in referencia.TakeWhile(char.IsLetter)) columna = columna * 26 + char.ToUpperInvariant(letra) - 'A' + 1;
                celdas[columna - 1] = tipo == "s" && int.TryParse(valor, out var indice) && indice < sharedStrings.Count ? sharedStrings[indice] : valor;
            }
            return Enumerable.Range(0, celdas.Count == 0 ? 0 : celdas.Keys.Max() + 1).Select(i => celdas.GetValueOrDefault(i, string.Empty)).ToList();
        }).ToList();
    }

    private static void ValidarLimites(List<List<string>> tabla)
    {
        if (tabla.Count > MaxRows + 1) throw new AppException($"El archivo no puede contener más de {MaxRows} filas de datos.");
        if (tabla.Any(fila => fila.Count > MaxColumns)) throw new AppException($"El archivo no puede contener más de {MaxColumns} columnas.");
        if (tabla.SelectMany(fila => fila).Any(valor => valor.Length > MaxCellLength))
            throw new AppException($"Cada celda debe contener como máximo {MaxCellLength} caracteres.");
    }

    private static List<FilaImportada> ConvertirTabla(List<List<string>> tabla)
    {
        if (tabla.Count == 0) throw new AppException("El archivo está vacío.");
        var indiceEncabezado = tabla.FindIndex(fila =>
        {
            var nombres = fila.Select(Normalizar).ToHashSet();
            return nombres.Contains(Normalizar("Asignatura")) &&
                   nombres.Contains(Normalizar("Cuatrimestre")) &&
                   (nombres.Contains(Normalizar("P.E.")) || nombres.Contains(Normalizar("PE")) || nombres.Contains(Normalizar("Programa Educativo"))) &&
                   nombres.Contains(Normalizar("Docente"));
        });
        if (indiceEncabezado < 0)
            throw new AppException("El archivo debe contener una fila de encabezados con Asignatura, Cuatrimestre, P.E. y Docente.");

        var encabezados = tabla[indiceEncabezado].Select((valor, indice) => new { Nombre = Normalizar(valor), indice }).ToDictionary(x => x.Nombre, x => x.indice);
        int Columna(params string[] nombres) => nombres.Select(Normalizar).Where(encabezados.ContainsKey).Select(n => encabezados[n]).DefaultIfEmpty(-1).First();
        var asignatura = Columna("Asignatura"); var cuatrimestre = Columna("Cuatrimestre"); var pe = Columna("P.E.", "PE", "Programa Educativo"); var docente = Columna("Docente");
        if (new[] { asignatura, cuatrimestre, pe, docente }.Any(x => x < 0)) throw new AppException("El archivo debe contener las columnas Asignatura, Cuatrimestre, P.E. y Docente.");
        string Valor(List<string> fila, int indice) => indice < fila.Count ? fila[indice].Trim() : string.Empty;
        return tabla.Skip(indiceEncabezado + 1)
            .Select((fila, indice) => new { fila, Numero = indice + 2 })
            .Where(x => x.fila.Any(valor => !string.IsNullOrWhiteSpace(valor)))
            .Select(x => new FilaImportada(x.Numero + indiceEncabezado, Valor(x.fila, asignatura), Valor(x.fila, cuatrimestre), Valor(x.fila, pe), Valor(x.fila, docente)))
            .ToList();
    }

    private sealed record FilaImportada(int Numero, string? Asignatura, string? Cuatrimestre, string? ProgramaEducativo, string? Docente);
    private sealed record GrupoImportado(int Cuatrimestre, string Letra) { public string Codigo => $"{Cuatrimestre}{Letra}"; }
    private sealed record DatosDocente(string Nombre, string ApellidoPaterno, string ApellidoMaterno)
    {
        public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}";
    }
}
