using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Xunit;
using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services.Tests;

public sealed class ComentariosCorreccionTests
{
    [Fact]
    public async Task Docente_y_revisor_crean_y_listan_el_historial()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision);
        var servicio = new ComentariosCorreccionService(escenario.Contexto);

        await servicio.CrearAsync(escenario.Planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "Comentario docente" }, escenario.Docente.Id);
        await servicio.CrearAsync(escenario.Planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "Comentario revisor" }, escenario.Revisor.Id);
        var resultado = await servicio.ListarAsync(escenario.Planeacion.PublicId, escenario.Docente.Id);

        Assert.False(resultado.OcultosPorAprobacion);
        Assert.Equal(2, resultado.Comentarios.Count);
        Assert.Equal(new[] { "Docente", "Revisor" }, resultado.Comentarios.Select(c => c.RolEnChat));
    }

    [Fact]
    public async Task Director_no_crea_comentarios_pero_puede_listarlos()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision);
        var servicio = new ComentariosCorreccionService(escenario.Contexto);
        await servicio.CrearAsync(escenario.Planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "Comentario docente" }, escenario.Docente.Id);

        await Assert.ThrowsAsync<AppException>(() => servicio.CrearAsync(
            escenario.Planeacion.PublicId,
            new CrearComentarioCorreccionDto { Mensaje = "Comentario director" },
            escenario.Director.Id));
        var resultado = await servicio.ListarAsync(escenario.Planeacion.PublicId, escenario.Director.Id);

        Assert.Single(resultado.Comentarios);
    }

    [Fact]
    public async Task Aprobada_oculta_mensajes_y_Reabierta_restaura_el_historial()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision);
        var servicio = new ComentariosCorreccionService(escenario.Contexto);
        await servicio.CrearAsync(escenario.Planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "Historial conservado" }, escenario.Docente.Id);

        escenario.Planeacion.Estado = EstadoPlaneacion.Aprobada;
        await escenario.Contexto.SaveChangesAsync();
        var oculto = await servicio.ListarAsync(escenario.Planeacion.PublicId, escenario.Director.Id);

        escenario.Planeacion.Estado = EstadoPlaneacion.Reabierta;
        await escenario.Contexto.SaveChangesAsync();
        var visible = await servicio.ListarAsync(escenario.Planeacion.PublicId, escenario.Director.Id);

        Assert.True(oculto.OcultosPorAprobacion);
        Assert.Empty(oculto.Comentarios);
        Assert.False(visible.OcultosPorAprobacion);
        Assert.Single(visible.Comentarios);
        Assert.Equal("Historial conservado", visible.Comentarios[0].Mensaje);
        Assert.Single(escenario.Contexto.ChatMensajes);
    }

    [Fact]
    public async Task Docente_no_asignado_no_puede_crear_ni_listar()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision);
        var noAsignado = new Usuario { Id = 20, Nombre = "Docente", ApellidoPaterno = "No asignado" };
        escenario.Contexto.Usuarios.Add(noAsignado);
        escenario.Contexto.UsuarioRoles.Add(new UsuarioRol { UsuarioId = noAsignado.Id, RolId = 1 });
        await escenario.Contexto.SaveChangesAsync();
        var servicio = new ComentariosCorreccionService(escenario.Contexto);

        await Assert.ThrowsAsync<ForbiddenException>(() => servicio.CrearAsync(
            escenario.Planeacion.PublicId,
            new CrearComentarioCorreccionDto { Mensaje = "Sin acceso" },
            noAsignado.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => servicio.ListarAsync(escenario.Planeacion.PublicId, noAsignado.Id));
    }

    [Theory]
    [InlineData(EstadoPlaneacion.Borrador)]
    [InlineData(EstadoPlaneacion.EnProceso)]
    [InlineData(EstadoPlaneacion.Aprobada)]
    [InlineData(EstadoPlaneacion.Rechazada)]
    [InlineData(EstadoPlaneacion.Finalizada)]
    public async Task Comentarios_solo_se_crean_en_estados_permitidos(EstadoPlaneacion estado)
    {
        await using var escenario = await CrearEscenarioAsync(estado);
        var servicio = new ComentariosCorreccionService(escenario.Contexto);

        await Assert.ThrowsAsync<AppException>(() => servicio.CrearAsync(
            escenario.Planeacion.PublicId,
            new CrearComentarioCorreccionDto { Mensaje = "No permitido" },
            escenario.Docente.Id));
    }

    [Theory]
    [InlineData(EstadoPlaneacion.EnRevision)]
    [InlineData(EstadoPlaneacion.CorreccionSolicitada)]
    [InlineData(EstadoPlaneacion.Reabierta)]
    public async Task Comentarios_se_crean_en_estados_permitidos(EstadoPlaneacion estado)
    {
        await using var escenario = await CrearEscenarioAsync(estado);

        var comentario = await new ComentariosCorreccionService(escenario.Contexto).CrearAsync(
            escenario.Planeacion.PublicId,
            new CrearComentarioCorreccionDto { Mensaje = "  Permitido  " },
            escenario.Docente.Id);

        Assert.Equal("Permitido", comentario.Mensaje);
    }

    [Fact]
    public async Task Comentario_rechaza_blancos_y_mas_de_4000_caracteres()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision);
        var servicio = new ComentariosCorreccionService(escenario.Contexto);

        await Assert.ThrowsAsync<AppException>(() => servicio.CrearAsync(
            escenario.Planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "   " }, escenario.Docente.Id));
        await Assert.ThrowsAsync<AppException>(() => servicio.CrearAsync(
            escenario.Planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = new string('x', 4001) }, escenario.Docente.Id));
    }

    [Fact]
    public async Task Revisor_no_puede_listar_comentarios_del_borrador_privado()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.Borrador);
        var servicio = new ComentariosCorreccionService(escenario.Contexto);

        await Assert.ThrowsAsync<ForbiddenException>(() => servicio.ListarAsync(escenario.Planeacion.PublicId, escenario.Revisor.Id));
        var comoDirector = await servicio.ListarAsync(escenario.Planeacion.PublicId, escenario.Director.Id);
        Assert.Empty(comoDirector.Comentarios);
    }

    [Fact]
    public async Task Aprobada_pasa_a_Reabierta_y_Reabierta_regresa_a_EnRevision()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.Aprobada);
        var autorizacion = new AutorizacionService(escenario.Contexto);
        var estados = new EstadoPlaneacionService(escenario.Contexto, autorizacion);

        var reabierta = await estados.ResolverRevisionAsync(
            escenario.Planeacion.PublicId,
            escenario.Revisor.Id,
            EstadoPlaneacion.Reabierta);
        var enRevision = await estados.EnviarARevisionAsync(escenario.Planeacion.PublicId, escenario.Docente.Id);

        Assert.Equal(EstadoPlaneacion.Reabierta, reabierta.Estado);
        Assert.Equal(EstadoPlaneacion.EnRevision, enRevision.Estado);
    }

    [Fact]
    public async Task Reabierta_no_puede_aprobarse_sin_volver_a_EnRevision()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.Reabierta);
        var estados = new EstadoPlaneacionService(escenario.Contexto, new AutorizacionService(escenario.Contexto));

        await Assert.ThrowsAsync<AppException>(() => estados.ResolverRevisionAsync(
            escenario.Planeacion.PublicId,
            escenario.Revisor.Id,
            EstadoPlaneacion.Aprobada));
    }

    [Fact]
    public async Task Revisor_puede_devolver_planeacion_a_correcciones()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision);
        var estados = new EstadoPlaneacionService(escenario.Contexto, new AutorizacionService(escenario.Contexto));

        var resultado = await estados.ResolverRevisionAsync(
            escenario.Planeacion.PublicId, escenario.Revisor.Id, EstadoPlaneacion.CorreccionSolicitada);

        Assert.Equal(EstadoPlaneacion.CorreccionSolicitada, resultado.Estado);
    }

    [Fact]
    public async Task Docente_Revisor_puede_comentar_y_resolver_su_misma_planeacion()
    {
        await using var escenario = await CrearEscenarioAsync(EstadoPlaneacion.EnRevision, autorrevisor: true);
        var comentarios = new ComentariosCorreccionService(escenario.Contexto);
        var estados = new EstadoPlaneacionService(escenario.Contexto, new AutorizacionService(escenario.Contexto));

        var comentario = await comentarios.CrearAsync(
            escenario.Planeacion.PublicId,
            new CrearComentarioCorreccionDto { Mensaje = "Comentario como autorrevisor" },
            escenario.Docente.Id);
        var aprobada = await estados.ResolverRevisionAsync(
            escenario.Planeacion.PublicId,
            escenario.Docente.Id,
            EstadoPlaneacion.Aprobada);

        Assert.Equal("Revisor", comentario.RolEnChat);
        Assert.Equal(EstadoPlaneacion.Aprobada, aprobada.Estado);
    }

    private static async Task<Escenario> CrearEscenarioAsync(EstadoPlaneacion estado, bool autorrevisor = false)
    {
        var contexto = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var docente = new Usuario { Id = 10, Nombre = "Diana", ApellidoPaterno = "Docente" };
        var revisor = autorrevisor
            ? docente
            : new Usuario { Id = 11, Nombre = "Roberto", ApellidoPaterno = "Revisor" };
        var director = new Usuario { Id = 12, Nombre = "Daniel", ApellidoPaterno = "Director" };
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        var rolRevisor = new RolEntidad { Id = 2, Nombre = "Revisor" };
        var rolDirector = new RolEntidad { Id = 3, Nombre = "Director" };
        contexto.AddRange(docente, director, rolDocente, rolRevisor, rolDirector);
        if (!autorrevisor) contexto.Usuarios.Add(revisor);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id },
            new UsuarioRol { UsuarioId = revisor.Id, RolId = rolRevisor.Id },
            new UsuarioRol { UsuarioId = director.Id, RolId = rolDirector.Id });
        contexto.Periodos.Add(new Periodo { Id = 1, Nombre = "Periodo de prueba", CicloEscolarId = 1 });
        contexto.Asignaturas.Add(new Asignatura { Id = 1, Nombre = "Asignatura de prueba", Clave = "AP-1", Cuatrimestre = 1 });
        var planeacion = new PlaneacionDidactica
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            PeriodoId = 1,
            AsignaturaId = 1,
            RevisorId = revisor.Id,
            Estado = estado
        };
        contexto.PlaneacionesDidacticas.Add(planeacion);
        contexto.CargasAcademicas.Add(new CargaAcademica
        {
            Id = 1,
            PeriodoId = planeacion.PeriodoId,
            GrupoId = 1,
            AsignaturaId = planeacion.AsignaturaId,
            DocenteId = docente.Id
        });
        await contexto.SaveChangesAsync();
        return new Escenario(contexto, planeacion, docente, revisor, director);
    }

    private sealed record Escenario(
        AppDbContext Contexto,
        PlaneacionDidactica Planeacion,
        Usuario Docente,
        Usuario Revisor,
        Usuario Director) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => Contexto.DisposeAsync();
    }
}
