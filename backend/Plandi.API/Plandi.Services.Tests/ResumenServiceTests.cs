using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services;
using Plandi.Services.Interfaces;
using Xunit;

namespace Plandi.Services.Tests;

public sealed class ResumenServiceTests
{
    private static readonly DateTime Hoy = new(2026, 8, 17);

    [Fact]
    public async Task Director_obtiene_conteos_y_seguimiento_del_periodo_actual()
    {
        await using var db = CrearContexto();
        var director = Usuario(1, "Director");
        var docente = Usuario(2, "Docente");
        var revisor = Usuario(3, "Revisor");
        var ciclo = new CicloEscolar { Id = 1, Nombre = "2026" };
        var periodo = new Periodo
        {
            Id = 1, Nombre = "Mayo-Agosto", CicloEscolar = ciclo, FechaInicio = Hoy.AddDays(-10), FechaFin = Hoy.AddDays(10),
            FechaLimiteEntregaPlaneaciones = Hoy.AddDays(1), Estado = EstadoPeriodo.Activo
        };
        var asignatura = new Asignatura { Id = 1, Nombre = "Programación", Clave = "PROG", Cuatrimestre = 1 };
        db.AddRange(director, docente, revisor, ciclo, periodo, asignatura,
            new Academia { Id = 1, Nombre = "TI" },
            new Grupo { Id = 1, Nombre = "1A", Cuatrimestre = 1, Carrera = new Carrera { Id = 1, Nombre = "TIC", Clave = "TIC" }, Periodo = periodo },
            new CargaAcademica { Id = 1, Periodo = periodo, Grupo = new Grupo { Id = 2, Nombre = "1B", Cuatrimestre = 1, Carrera = new Carrera { Id = 2, Nombre = "MEC", Clave = "MEC" }, Periodo = periodo }, Asignatura = asignatura, Docente = docente },
            new PlaneacionDidactica { Id = 1, Periodo = periodo, Asignatura = asignatura, Estado = EstadoPlaneacion.Borrador, Revisor = revisor },
            new PlaneacionDidactica { Id = 2, Periodo = periodo, Asignatura = new Asignatura { Id = 2, Nombre = "Bases", Clave = "BD", Cuatrimestre = 1 }, Estado = EstadoPlaneacion.Aprobada, Revisor = revisor });
        await db.SaveChangesAsync();

        var service = Servicio(db);
        var dashboard = await service.ObtenerDashboardAsync();
        var seguimiento = await service.ObtenerSeguimientoPlaneacionesAsync();

        Assert.Equal(3, dashboard.UsuariosRegistrados);
        Assert.Equal(1, dashboard.Importaciones);
        Assert.Equal(50m, dashboard.AvancePlaneaciones);
        Assert.Equal(2, seguimiento.Total);
        Assert.Equal(1, seguimiento.Completadas);
        Assert.Equal(1, seguimiento.PorVencer);
    }

    [Fact]
    public async Task Resumenes_de_docente_y_revisor_respetan_sus_asignaciones_y_estados()
    {
        await using var db = CrearContexto();
        var docente = Usuario(1, "Docente");
        var otroDocente = Usuario(2, "Docente");
        var revisor = Usuario(3, "Revisor");
        var otroRevisor = Usuario(4, "Revisor");
        var periodo = new Periodo { Id = 1, Nombre = "Actual", FechaInicio = Hoy.AddDays(-1), FechaFin = Hoy.AddDays(1), Estado = EstadoPeriodo.Activo };
        var asignatura = new Asignatura { Id = 1, Nombre = "Uno", Clave = "A1", Cuatrimestre = 1 };
        var otraAsignatura = new Asignatura { Id = 2, Nombre = "Dos", Clave = "A2", Cuatrimestre = 1 };
        db.AddRange(docente, otroDocente, revisor, otroRevisor, periodo, asignatura, otraAsignatura,
            new CargaAcademica { Id = 1, Periodo = periodo, Grupo = Grupo(1, periodo), Asignatura = asignatura, Docente = docente },
            new CargaAcademica { Id = 2, Periodo = periodo, Grupo = Grupo(2, periodo), Asignatura = otraAsignatura, Docente = otroDocente },
            new PlaneacionDidactica { Id = 1, Periodo = periodo, Asignatura = asignatura, Estado = EstadoPlaneacion.CorreccionSolicitada, Revisor = revisor },
            new PlaneacionDidactica { Id = 2, Periodo = periodo, Asignatura = otraAsignatura, Estado = EstadoPlaneacion.EnRevision, Revisor = otroRevisor });
        await db.SaveChangesAsync();

        var service = Servicio(db);
        var docenteResumen = await service.ObtenerPlaneacionesDocenteAsync(docente.Id);
        var revisorResumen = await service.ObtenerDashboardRevisorAsync(revisor.Id);

        Assert.Equal(1, docenteResumen.Total);
        Assert.Equal(1, docenteResumen.Borrador);
        Assert.Equal(0, revisorResumen.PlaneacionesAValidar);
        Assert.Equal(1, revisorResumen.Correcciones);
    }

    private static ResumenService Servicio(AppDbContext db) => new(db, new AutorizacionService(db), new RelojPrueba(Hoy));
    private static AppDbContext CrearContexto() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static Usuario Usuario(long id, string rol) => new()
    {
        Id = id, Nombre = $"Usuario{id}", ApellidoPaterno = "Prueba", Email = $"u{id}@test.mx",
        UsuarioRoles = [new UsuarioRol { Rol = new Plandi.Library.Models.Rol { Id = id, Nombre = rol } }]
    };
    private static Grupo Grupo(long id, Periodo periodo) => new() { Id = id, Nombre = $"G{id}", Cuatrimestre = 1, Periodo = periodo, Carrera = new Carrera { Id = id + 10, Nombre = $"Carrera{id}", Clave = $"C{id}" } };

    private sealed class RelojPrueba(DateTime ahora) : IRelojAcademico
    {
        public DateTime AhoraLocal => ahora;
        public DateTime AhoraUtc => DateTime.SpecifyKind(ahora, DateTimeKind.Utc);
    }
}
