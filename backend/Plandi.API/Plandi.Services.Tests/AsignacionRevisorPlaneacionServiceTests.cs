using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services;
using Xunit;
using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services.Tests;

public sealed class AsignacionRevisorPlaneacionServiceTests
{
    [Fact]
    public async Task Director_lista_planeaciones_activas_con_estado_y_revisor_real()
    {
        await using var context = CrearContexto();
        var escenario = await PrepararEscenarioAsync(context);
        var service = new AsignacionRevisorPlaneacionService(context, new AutorizacionService(context));

        var result = await service.ObtenerAsync(escenario.Director.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(escenario.ConRevisor.PublicId, result[0].PublicId);
        Assert.Equal(EstadoPlaneacion.EnRevision, result[0].Estado);
        Assert.Equal(escenario.Revisor.PublicId, result[0].RevisorPublicId);
        Assert.Equal("Revisor Prueba", result[0].Revisor);
        Assert.Equal("Docente Prueba", result[0].Docentes);
        Assert.Equal(escenario.SinRevisor.PublicId, result[1].PublicId);
        Assert.Null(result[1].RevisorPublicId);
        Assert.Null(result[1].Revisor);
        Assert.DoesNotContain(result, item => item.PublicId == escenario.Inactiva.PublicId || item.PublicId == escenario.Eliminada.PublicId);
    }

    [Theory]
    [InlineData("Docente")]
    [InlineData("Revisor")]
    public async Task Usuario_sin_rol_Director_no_puede_listar_asignaciones(string rol)
    {
        await using var context = CrearContexto();
        var escenario = await PrepararEscenarioAsync(context);
        var usuario = rol == "Docente" ? escenario.Docente : escenario.Revisor;
        var service = new AsignacionRevisorPlaneacionService(context, new AutorizacionService(context));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ObtenerAsync(usuario.Id));
    }

    private static AppDbContext CrearContexto() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<Escenario> PrepararEscenarioAsync(AppDbContext context)
    {
        var director = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Directora", ApellidoPaterno = "Prueba" };
        var docente = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Docente", ApellidoPaterno = "Prueba" };
        var revisor = new Usuario { Id = 3, PublicId = Guid.NewGuid(), Nombre = "Revisor", ApellidoPaterno = "Prueba" };
        var rolDirector = new RolEntidad { Id = 1, Nombre = "Director" };
        var rolDocente = new RolEntidad { Id = 2, Nombre = "Docente" };
        var rolRevisor = new RolEntidad { Id = 3, Nombre = "Revisor" };
        var periodo = new Periodo { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Mayo-Agosto 2026", CicloEscolarId = 1 };
        var asignatura = new Asignatura { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Asignatura", Clave = "ASG", Cuatrimestre = 1 };
        var otraAsignatura = new Asignatura { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Otra asignatura", Clave = "OTR", Cuatrimestre = 1 };
        var conRevisor = Planeacion(1, periodo, asignatura, "Docente Prueba", EstadoPlaneacion.EnRevision, new DateTime(2026, 8, 2), revisor);
        var sinRevisor = Planeacion(2, periodo, otraAsignatura, "Docente Prueba", EstadoPlaneacion.Borrador, new DateTime(2026, 8, 1));
        var inactiva = Planeacion(3, periodo, new Asignatura { Id = 3, Nombre = "Inactiva", Clave = "INA", Cuatrimestre = 1 }, "Docente Prueba", EstadoPlaneacion.Aprobada, new DateTime(2026, 8, 3));
        inactiva.Activo = false;
        var eliminada = Planeacion(4, periodo, new Asignatura { Id = 4, Nombre = "Eliminada", Clave = "DEL", Cuatrimestre = 1 }, "Docente Prueba", EstadoPlaneacion.Rechazada, new DateTime(2026, 8, 4));
        eliminada.DeletedAt = DateTime.UtcNow;

        context.AddRange(director, docente, revisor, rolDirector, rolDocente, rolRevisor, periodo, asignatura, otraAsignatura, conRevisor, sinRevisor, inactiva, eliminada);
        context.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = director.Id, RolId = rolDirector.Id },
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id },
            new UsuarioRol { UsuarioId = revisor.Id, RolId = rolRevisor.Id });
        await context.SaveChangesAsync();

        return new Escenario(director, docente, revisor, conRevisor, sinRevisor, inactiva, eliminada);
    }

    private static PlaneacionDidactica Planeacion(long id, Periodo periodo, Asignatura asignatura, string docentes, EstadoPlaneacion estado, DateTime updatedAt, Usuario? revisor = null) => new()
    {
        Id = id,
        PublicId = Guid.NewGuid(),
        Periodo = periodo,
        Asignatura = asignatura,
        Caratula = new PlaneacionCaratula { Docentes = docentes },
        Estado = estado,
        UpdatedAt = updatedAt,
        Revisor = revisor,
        RevisorId = revisor?.Id
    };

    private sealed record Escenario(Usuario Director, Usuario Docente, Usuario Revisor, PlaneacionDidactica ConRevisor, PlaneacionDidactica SinRevisor, PlaneacionDidactica Inactiva, PlaneacionDidactica Eliminada);
}
