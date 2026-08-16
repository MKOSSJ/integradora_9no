using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class GeneracionPlaneacionesService(AppDbContext dbContext, ILogger<GeneracionPlaneacionesService> logger) : IGeneracionPlaneacionesService
{
    public async Task<GeneracionPlaneacionesResultadoDto> GenerarAsync(CancellationToken cancellationToken = default)
    {
        var resultado = new GeneracionPlaneacionesResultadoDto();
        var programas = (await dbContext.ProgramasAsignatura.Include(p => p.Asignatura)
            .Where(p => p.Activo && p.DeletedAt == null && p.AsignaturaId != null && p.JsonExtraido != null)
            .ToListAsync(cancellationToken))
            // A planeación belongs to the subject. If its program was re-imported,
            // only the latest extracted version is used as the initial snapshot.
            .GroupBy(programa => programa.AsignaturaId)
            .Select(group => group.OrderByDescending(programa => programa.UpdatedAt ?? programa.CreatedAt).First())
            .ToList();
        resultado.TotalProgramas = programas.Count;

        foreach (var programa in programas)
        {
            await using var transaccion = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var cargas = await dbContext.CargasAcademicas.Include(c => c.Periodo).Include(c => c.Grupo).ThenInclude(g => g.Carrera)
                    .Include(c => c.Docente).Where(c => c.Activo && c.DeletedAt == null && c.AsignaturaId == programa.AsignaturaId).ToListAsync(cancellationToken);
                if (cargas.Count == 0)
                {
                    Agregar(resultado, programa, "omitida", "No existe una carga académica activa para esta asignatura.");
                    resultado.Omitidas++; await transaccion.CommitAsync(cancellationToken); continue;
                }

                var porPeriodo = cargas.GroupBy(c => c.PeriodoId);
                foreach (var grupoPeriodo in porPeriodo)
                {
                    var carga = grupoPeriodo.First();
                    var existente = await dbContext.PlaneacionesDidacticas.FirstOrDefaultAsync(p => p.Activo && p.DeletedAt == null && p.PeriodoId == carga.PeriodoId && p.AsignaturaId == carga.AsignaturaId, cancellationToken);
                    if (existente is not null) { resultado.YaExistentes++; Agregar(resultado, programa, "existente", null, existente.PublicId); continue; }

                    var datos = JsonSerializer.Deserialize<ProgramaAsignaturaExtraidoDto>(programa.JsonExtraido!) ?? throw new InvalidOperationException("El JSON del programa no es válido.");
                    var planeacion = CrearPlaneacion(programa, carga, grupoPeriodo.ToList(), datos);
                    dbContext.PlaneacionesDidacticas.Add(planeacion);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    resultado.PlaneacionesCreadas++;
                    Agregar(resultado, programa, "creada", null, planeacion.PublicId);
                }
                await transaccion.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync(cancellationToken);
                logger.LogError(ex, "No fue posible generar planeación para programa {ProgramaId}", programa.PublicId);
                resultado.Omitidas++; Agregar(resultado, programa, "error", ex.Message);
                dbContext.ChangeTracker.Clear();
            }
        }
        return resultado;
    }

    private static PlaneacionDidactica CrearPlaneacion(ProgramaAsignatura programa, CargaAcademica carga, List<CargaAcademica> cargas, ProgramaAsignaturaExtraidoDto datos)
    {
        var planeacion = new PlaneacionDidactica
        {
            PeriodoId = carga.PeriodoId,
            AsignaturaId = carga.AsignaturaId,
            AcademiaId = programa.AcademiaId ?? carga.AcademiaId,
            CreatedBy = carga.DocenteId,
            Estado = EstadoPlaneacion.Borrador
        };
        planeacion.Caratula = new PlaneacionCaratula
        {
            ProgramaAsignaturaId = programa.Id, ProgramaEducativo = carga.Grupo.Carrera.Nombre, Cuatrimestre = programa.Cuatrimestre,
            NombreAsignatura = programa.NombreAsignatura, Docentes = string.Join(", ", cargas.Select(c => $"{c.Docente.Nombre} {c.Docente.ApellidoPaterno} {c.Docente.ApellidoMaterno}".Trim()).Distinct()),
            PeriodoEscolar = carga.Periodo.Nombre, Grupos = string.Join(", ", cargas.Select(c => c.Grupo.Nombre).Distinct()),
            PropositoAsignatura = programa.Proposito, CompetenciaAsignatura = programa.Competencia, TipoCompetencia = datos.TipoCompetencia,
            Creditos = programa.Creditos, Modalidad = datos.Modalidad, HorasSaber = datos.HorasSaber, HorasSaberHacer = datos.HorasSaberHacer,
            HorasTotales = programa.HorasTotales, HorasSemana = programa.HorasSemana
        };
        foreach (var unidadFuente in datos.Unidades.OrderBy(unidad => unidad.Numero))
        {
            var unidad = new PlaneacionUnidad
            {
                NumeroUnidad = unidadFuente.Numero,
                NombreUnidad = unidadFuente.Nombre,
                PropositoEsperado = unidadFuente.Proposito,
                HorasSaber = unidadFuente.HorasSaber,
                HorasSaberHacer = unidadFuente.HorasSaberHacer,
                HorasTotales = unidadFuente.HorasTotales,
                PorcentajeUnidad = CalcularPorcentaje(unidadFuente.HorasTotales, datos.HorasTotales),
                Orden = unidadFuente.Numero
            };

            // Las etapas son estructura obligatoria, no contenido opcional del
            // docente. Se crean aunque no existan todavía actividades.
            unidad.EtapasSecuencia.Add(new PlaneacionEtapaSecuencia { Fase = FaseSecuencia.Apertura });
            unidad.EtapasSecuencia.Add(new PlaneacionEtapaSecuencia { Fase = FaseSecuencia.Desarrollo });
            unidad.EtapasSecuencia.Add(new PlaneacionEtapaSecuencia { Fase = FaseSecuencia.Cierre });

            foreach (var (temaFuente, indice) in unidadFuente.Temas.Select((tema, indice) => (tema, indice)))
            {
                unidad.Temas.Add(new PlaneacionTema
                {
                    Tema = temaFuente.Nombre,
                    SaberConceptual = temaFuente.Saber,
                    SaberHacer = temaFuente.SaberHacer,
                    SaberSer = temaFuente.SerConvivir,
                    Orden = indice + 1
                });
            }

            var evaluacionFuente = unidadFuente.ProcesoEvaluacion;
            if (TieneEvaluacion(evaluacionFuente))
            {
                unidad.Evaluaciones.Add(new PlaneacionEvaluacion
                {
                    ResultadoAprendizaje = evaluacionFuente.ResultadoAprendizaje,
                    EvidenciaAprendizaje = evaluacionFuente.EvidenciaAprendizaje,
                    InstrumentoEvaluacion = evaluacionFuente.InstrumentosEvaluacion,
                    Fase = FaseSecuencia.Desarrollo,
                    AgenteEvaluador = AgenteEvaluador.Heteroevaluacion,
                    Orden = 1
                });
            }

            planeacion.Unidades.Add(unidad);
        }

        var ordenReferencia = 1;
        foreach (var referencia in datos.ReferenciasBibliograficas.Select(FormatearReferencia).Where(referencia => !string.IsNullOrWhiteSpace(referencia))
                     .Concat(datos.ReferenciasDigitales.Select(FormatearReferencia).Where(referencia => !string.IsNullOrWhiteSpace(referencia))))
        {
            planeacion.Referencias.Add(new PlaneacionReferencia { ReferenciaAPA = referencia!, Orden = ordenReferencia++ });
        }
        return planeacion;
    }

    private static decimal? CalcularPorcentaje(int? horasUnidad, int? horasAsignatura) =>
        horasUnidad is > 0 && horasAsignatura is > 0
            ? decimal.Round(horasUnidad.Value * 100m / horasAsignatura.Value, 2)
            : null;

    private static bool TieneEvaluacion(ProcesoEvaluacionUnidadExtraidoDto evaluacion) =>
        !string.IsNullOrWhiteSpace(evaluacion.ResultadoAprendizaje) ||
        !string.IsNullOrWhiteSpace(evaluacion.EvidenciaAprendizaje) ||
        !string.IsNullOrWhiteSpace(evaluacion.InstrumentosEvaluacion);

    private static string? FormatearReferencia(ReferenciaBibliograficaExtraidaDto referencia)
    {
        var partes = new[]
        {
            Unir(referencia.Autor, referencia.Anio is null ? null : $"({referencia.Anio})."),
            referencia.Titulo,
            Unir(referencia.LugarPublicacion, referencia.Editorial),
            referencia.Isbn is null ? null : $"ISBN: {referencia.Isbn}"
        }.Where(parte => !string.IsNullOrWhiteSpace(parte));
        var texto = string.Join(" ", partes);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string? FormatearReferencia(ReferenciaDigitalExtraidaDto referencia)
    {
        var partes = new[]
        {
            referencia.Autor,
            referencia.FechaRecuperacion is null ? null : $"({referencia.FechaRecuperacion}).",
            referencia.Titulo,
            referencia.Vinculo
        }.Where(parte => !string.IsNullOrWhiteSpace(parte));
        var texto = string.Join(" ", partes);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string? Unir(string? izquierda, string? derecha) => string.Join(" ", new[] { izquierda, derecha }.Where(valor => !string.IsNullOrWhiteSpace(valor)));

    private static void Agregar(GeneracionPlaneacionesResultadoDto resultado, ProgramaAsignatura programa, string estado, string? mensaje, Guid? planeacionId = null) =>
        resultado.Planeaciones.Add(new GeneracionPlaneacionDetalleDto { ProgramaAsignaturaPublicId = programa.PublicId, Asignatura = programa.NombreAsignatura, Estado = estado, Mensaje = mensaje, PlaneacionPublicId = planeacionId });
}
