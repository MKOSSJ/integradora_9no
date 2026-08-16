using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Xunit;

namespace Plandi.Services.Tests;

public sealed class AcademicLifecycleTests
{
    private static readonly DateTime Hoy = new(2026, 4, 15, 12, 0, 0);

    [Fact]
    public async Task Periodo_vencido_se_detecta_y_se_cierra_aunque_estado_siga_activo()
    {
        await using var db = Contexto();
        var periodo = new Periodo { Id = 1, CicloEscolarId = 1, Nombre = "Enero-Abril", FechaInicio = new(2026, 1, 1), FechaFin = new(2026, 4, 14), Estado = EstadoPeriodo.Activo };
        db.Periodos.Add(periodo);
        await db.SaveChangesAsync();
        var service = Lifecycle(db);

        Assert.Equal(EstadoPeriodo.Cerrado, service.ObtenerEstadoEfectivo(periodo));
        await Assert.ThrowsAsync<ConflictException>(() => service.ExigirEditableAsync(periodo.Id));
        Assert.Equal(1, await service.ActualizarEstadosAsync());
        Assert.Equal(EstadoPeriodo.Cerrado, periodo.Estado);
        Assert.NotNull(periodo.FechaCierre);
    }

    [Fact]
    public async Task Periodo_vigente_permite_edicion_y_programado_no()
    {
        await using var db = Contexto();
        db.Periodos.AddRange(
            new Periodo { Id = 1, CicloEscolarId = 1, Nombre = "Vigente", FechaInicio = new(2026, 4, 1), FechaFin = new(2026, 4, 30), Estado = EstadoPeriodo.Activo },
            new Periodo { Id = 2, CicloEscolarId = 1, Nombre = "Futuro", FechaInicio = new(2026, 5, 1), FechaFin = new(2026, 8, 31), Estado = EstadoPeriodo.Programado });
        await db.SaveChangesAsync();
        var service = Lifecycle(db);

        await service.ExigirEditableAsync(1);
        await Assert.ThrowsAsync<ConflictException>(() => service.ExigirEditableAsync(2));
    }

    [Fact]
    public async Task Cambiar_grupo_valida_periodo_compatibilidad_y_duplicados()
    {
        await using var db = await ContextoCargaAsync(cerrado: false, incluirDuplicado: false);
        var service = new CargaAcademicaService(db, Lifecycle(db));
        var carga = await db.CargasAcademicas.SingleAsync();
        var grupoNuevo = await db.Grupos.SingleAsync(x => x.Nombre == "9B");

        var result = await service.UpdateGrupo(carga.PublicId, new ActualizarGrupoCargaAcademicaDto { GrupoPublicId = grupoNuevo.PublicId }, 99);

        Assert.Equal(grupoNuevo.PublicId, result.GrupoPublicId);
        Assert.Equal(grupoNuevo.Id, carga.GrupoId);
        Assert.Equal(99, carga.UpdatedBy);
    }

    [Fact]
    public async Task Cambiar_grupo_en_periodo_cerrado_es_conflicto()
    {
        await using var db = await ContextoCargaAsync(cerrado: true, incluirDuplicado: false);
        var service = new CargaAcademicaService(db, Lifecycle(db));
        var carga = await db.CargasAcademicas.SingleAsync();
        var grupoNuevo = await db.Grupos.SingleAsync(x => x.Nombre == "9B");

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateGrupo(carga.PublicId, new ActualizarGrupoCargaAcademicaDto { GrupoPublicId = grupoNuevo.PublicId }, 99));
    }

    [Fact]
    public async Task Planeacion_en_borrador_de_periodo_cerrado_tambien_es_solo_lectura()
    {
        await using var db = await ContextoCargaAsync(cerrado: true, incluirDuplicado: false);
        var carga = await db.CargasAcademicas.SingleAsync();
        var planeacion = new PlaneacionDidactica { PeriodoId = carga.PeriodoId, AsignaturaId = carga.AsignaturaId, Estado = EstadoPlaneacion.Borrador };
        db.PlaneacionesDidacticas.Add(planeacion);
        await db.SaveChangesAsync();
        var service = new PlaneacionReferenciaService(db, null!, Lifecycle(db));

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(planeacion.PublicId, new Plandi.Dto.CreatePlaneacionReferenciaDto { ReferenciaAPA = "Referencia", Orden = 1 }));
    }

    [Fact]
    public async Task Cambiar_grupo_no_permite_duplicar_asignacion()
    {
        await using var db = await ContextoCargaAsync(cerrado: false, incluirDuplicado: true);
        var service = new CargaAcademicaService(db, Lifecycle(db));
        var carga = await db.CargasAcademicas.SingleAsync(x => x.Grupo.Nombre == "9A");
        var grupoNuevo = await db.Grupos.SingleAsync(x => x.Nombre == "9B");

        await Assert.ThrowsAsync<AppException>(() => service.UpdateGrupo(carga.PublicId, new ActualizarGrupoCargaAcademicaDto { GrupoPublicId = grupoNuevo.PublicId }, 99));
    }

    [Fact]
    public async Task Consulta_administrativa_de_cargas_devuelve_relaciones_humanas_y_paginacion()
    {
        await using var db = await ContextoCargaAsync(cerrado: false, incluirDuplicado: false);
        var service = new AdministracionAcademicaService(db, Lifecycle(db));

        var result = await service.CargasAsync(new AdminConsultaDto { Search = "Ana", Page = 1, PageSize = 10 });

        var carga = Assert.Single(result.Items);
        Assert.Equal("Ana Pérez", carga.Docente.Nombre);
        Assert.Equal("Web", carga.Asignatura.Nombre);
        Assert.Equal("9A", carga.Grupo.Nombre);
        Assert.Equal("TIC", carga.Programa.Clave);
        Assert.Equal(1, result.TotalItems);
        Assert.True(carga.PermiteModificaciones);
    }

    [Fact]
    public async Task Repositorio_combina_filtros_y_excluye_periodos_activos()
    {
        await using var db = await ContextoRepositorioAsync();
        var director = await db.Usuarios.SingleAsync(x => x.Nombre == "Directora");
        var docente = await db.Usuarios.SingleAsync(x => x.Nombre == "Ana");
        var periodo = await db.Periodos.SingleAsync(x => x.Estado == EstadoPeriodo.Cerrado);
        var asignatura = await db.Asignaturas.SingleAsync(x => x.Clave == "WEB");
        var service = Repositorio(db);

        var result = await service.BuscarAsync(new RepositorioPlaneacionesFiltroDto
        {
            PeriodoPublicId = periodo.PublicId,
            AsignaturaPublicId = asignatura.PublicId,
            DocentePublicId = docente.PublicId,
            Page = 1,
            PageSize = 10
        }, director.Id);

        Assert.Single(result.Items);
        Assert.Equal("WEB", result.Items[0].Asignatura.Clave);
        Assert.True(result.Items[0].SoloLectura);
        Assert.Equal(EstadoPeriodo.Cerrado, result.Items[0].Periodo.Estado);
    }

    [Fact]
    public async Task Repositorio_docente_solo_ve_sus_planeaciones_historicas_y_descarga_pdf()
    {
        await using var db = await ContextoRepositorioAsync();
        var docente = await db.Usuarios.SingleAsync(x => x.Nombre == "Ana");
        var service = Repositorio(db);
        var result = await service.BuscarAsync(new RepositorioPlaneacionesFiltroDto(), docente.Id);
        var planeacion = Assert.Single(result.Items);

        var archivo = await service.DescargarAsync(planeacion.PublicId, "planeacion", docente.Id);

        Assert.Equal("application/pdf", archivo.MimeType);
        Assert.NotEmpty(archivo.Bytes);
    }

    [Fact]
    public async Task Repositorio_descarga_programa_que_pertenece_a_la_planeacion()
    {
        await using var db = await ContextoRepositorioAsync();
        var ruta = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(ruta, [4, 5, 6]);
            var director = await db.Usuarios.SingleAsync(x => x.Nombre == "Directora");
            var planeacion = await db.PlaneacionesDidacticas.Include(x => x.Asignatura).SingleAsync(x => x.Periodo.Estado == EstadoPeriodo.Cerrado);
            var documento = new Documento
            {
                TipoDocumento = TipoDocumento.ProgramaAsignatura, Titulo = "Programa", NombreOriginal = "programa.pdf", NombreGuardado = "programa.pdf",
                Extension = ".pdf", MimeType = "application/pdf", TamanoBytes = 3, RutaStorage = ruta, SubidoPor = director, Estado = EstadoDocumento.Procesado
            };
            var programa = new ProgramaAsignatura { Documento = documento, Asignatura = planeacion.Asignatura, NombreAsignatura = planeacion.Asignatura.Nombre };
            planeacion.Caratula = new PlaneacionCaratula { ProgramaAsignatura = programa };
            await db.SaveChangesAsync();

            var archivo = await Repositorio(db).DescargarAsync(planeacion.PublicId, "programa", director.Id);

            Assert.Equal("programa.pdf", archivo.NombreDescarga);
            Assert.Equal(new byte[] { 4, 5, 6 }, archivo.Bytes);
        }
        finally
        {
            File.Delete(ruta);
        }
    }

    private static AppDbContext Contexto() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static IPeriodoLifecycleService Lifecycle(AppDbContext db) => new PeriodoLifecycleService(db, new RelojPrueba(Hoy));

    private static async Task<AppDbContext> ContextoCargaAsync(bool cerrado, bool incluirDuplicado)
    {
        var db = Contexto();
        var ciclo = new CicloEscolar { Id = 1, PublicId = Guid.NewGuid(), Nombre = "2026", FechaInicio = new(2026, 1, 1), FechaFin = new(2026, 12, 31) };
        var periodo = new Periodo { Id = 1, PublicId = Guid.NewGuid(), CicloEscolar = ciclo, Nombre = "Periodo", FechaInicio = new(2026, 1, 1), FechaFin = cerrado ? new(2026, 4, 14) : new(2026, 4, 30), Estado = cerrado ? EstadoPeriodo.Cerrado : EstadoPeriodo.Activo };
        var carrera = new Carrera { Id = 1, PublicId = Guid.NewGuid(), Nombre = "TIC", Clave = "TIC" };
        var grupoA = new Grupo { Id = 1, PublicId = Guid.NewGuid(), Nombre = "9A", Cuatrimestre = 9, Carrera = carrera, Periodo = periodo };
        var grupoB = new Grupo { Id = 2, PublicId = Guid.NewGuid(), Nombre = "9B", Cuatrimestre = 9, Carrera = carrera, Periodo = periodo };
        var asignatura = new Asignatura { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Web", Clave = "WEB", Cuatrimestre = 9 };
        var docente = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Ana", ApellidoPaterno = "Pérez" };
        db.CargasAcademicas.Add(new CargaAcademica { Id = 1, PublicId = Guid.NewGuid(), Periodo = periodo, Grupo = grupoA, Asignatura = asignatura, Docente = docente });
        if (incluirDuplicado) db.CargasAcademicas.Add(new CargaAcademica { Id = 2, PublicId = Guid.NewGuid(), Periodo = periodo, Grupo = grupoB, Asignatura = asignatura, Docente = docente });
        else db.Grupos.Add(grupoB);
        await db.SaveChangesAsync();
        return db;
    }

    private static async Task<AppDbContext> ContextoRepositorioAsync()
    {
        var db = Contexto();
        var rolDirector = new Plandi.Library.Models.Rol { Id = 1, Nombre = "Director" };
        var rolDocente = new Plandi.Library.Models.Rol { Id = 2, Nombre = "Docente" };
        var director = new Usuario { Id = 10, Nombre = "Directora", ApellidoPaterno = "Uno" };
        var docente = new Usuario { Id = 20, Nombre = "Ana", ApellidoPaterno = "Pérez" };
        director.UsuarioRoles.Add(new UsuarioRol { Usuario = director, Rol = rolDirector });
        docente.UsuarioRoles.Add(new UsuarioRol { Usuario = docente, Rol = rolDocente });
        db.Usuarios.Add(director);
        var ciclo = new CicloEscolar { Id = 1, Nombre = "2026", FechaInicio = new(2026, 1, 1), FechaFin = new(2026, 12, 31) };
        var cerrado = new Periodo { Id = 1, Nombre = "Enero-Abril", CicloEscolar = ciclo, FechaInicio = new(2026, 1, 1), FechaFin = new(2026, 4, 14), Estado = EstadoPeriodo.Cerrado };
        var activo = new Periodo { Id = 2, Nombre = "Mayo-Agosto", CicloEscolar = ciclo, FechaInicio = new(2026, 4, 15), FechaFin = new(2026, 8, 31), Estado = EstadoPeriodo.Activo };
        var carrera = new Carrera { Id = 1, Nombre = "TIC", Clave = "TIC" };
        var grupo = new Grupo { Id = 1, Nombre = "9A", Cuatrimestre = 9, Carrera = carrera, Periodo = cerrado };
        var asignatura = new Asignatura { Id = 1, Nombre = "Programación Web", Clave = "WEB", Cuatrimestre = 9 };
        db.CargasAcademicas.Add(new CargaAcademica { Id = 1, Periodo = cerrado, Grupo = grupo, Asignatura = asignatura, Docente = docente });
        db.PlaneacionesDidacticas.AddRange(
            new PlaneacionDidactica { Id = 1, Periodo = cerrado, Asignatura = asignatura, Estado = EstadoPlaneacion.Aprobada },
            new PlaneacionDidactica { Id = 2, Periodo = activo, Asignatura = new Asignatura { Id = 2, Nombre = "Bases", Clave = "BD", Cuatrimestre = 9 }, Estado = EstadoPlaneacion.Borrador });
        await db.SaveChangesAsync();
        return db;
    }

    private static IRepositorioService Repositorio(AppDbContext db) => new RepositorioService(db, new AutorizacionService(db), Lifecycle(db), new RelojPrueba(Hoy), new PdfPrueba());

    private sealed class RelojPrueba(DateTime ahora) : IRelojAcademico
    {
        public DateTime AhoraLocal => ahora;
        public DateTime AhoraUtc => DateTime.SpecifyKind(ahora, DateTimeKind.Utc);
    }

    private sealed class PdfPrueba : IPlaneacionPdfService
    {
        public Task<ArchivoContenido> GenerarPdfAsync(Guid planeacionPublicId, long usuarioId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ArchivoContenido([1, 2, 3], "application/pdf", "planeacion.pdf"));
    }
}
