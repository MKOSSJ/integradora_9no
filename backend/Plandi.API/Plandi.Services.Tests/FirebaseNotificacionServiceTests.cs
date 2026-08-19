using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services;
using Xunit;

using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services.Tests;

public sealed class FirebaseNotificacionServiceTests
{
    private static AppDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task RegistrarTokenAsync_crea_nuevo_token_si_no_existe()
    {
        await using var contexto = CrearContexto();
        var logger = NullLogger<FirebaseNotificacionService>.Instance;
        var servicio = new FirebaseNotificacionService(contexto, logger);

        var dto = new RegistrarDispositivoDto
        {
            FcmToken = "fcm_token_12345",
            DeviceType = "android"
        };

        await servicio.RegistrarTokenAsync(100, dto);

        var tokenEnBd = await contexto.UserDeviceTokens.FirstOrDefaultAsync(t => t.DeviceToken == "fcm_token_12345");
        Assert.NotNull(tokenEnBd);
        Assert.Equal(100, tokenEnBd.UserId);
        Assert.Equal("android", tokenEnBd.DeviceType);
        Assert.True(tokenEnBd.Activo);
        Assert.Null(tokenEnBd.DeletedAt);
    }

    [Fact]
    public async Task RegistrarTokenAsync_actualiza_token_existente_y_lo_reactiva()
    {
        await using var contexto = CrearContexto();
        var tokenExistente = new UserDeviceToken
        {
            UserId = 50,
            DeviceToken = "fcm_token_existente",
            DeviceType = "ios",
            Activo = false,
            DeletedAt = DateTime.UtcNow.AddDays(-10)
        };
        contexto.UserDeviceTokens.Add(tokenExistente);
        await contexto.SaveChangesAsync();

        var logger = NullLogger<FirebaseNotificacionService>.Instance;
        var servicio = new FirebaseNotificacionService(contexto, logger);

        var dto = new RegistrarDispositivoDto
        {
            FcmToken = "fcm_token_existente",
            DeviceType = "web"
        };

        await servicio.RegistrarTokenAsync(200, dto);

        var tokenActualizado = await contexto.UserDeviceTokens.FirstOrDefaultAsync(t => t.DeviceToken == "fcm_token_existente");
        Assert.NotNull(tokenActualizado);
        Assert.Equal(200, tokenActualizado.UserId);
        Assert.Equal("web", tokenActualizado.DeviceType);
        Assert.True(tokenActualizado.Activo);
        Assert.Null(tokenActualizado.DeletedAt);
        Assert.NotNull(tokenActualizado.UpdatedAt);
    }

    [Fact]
    public async Task RegistrarTokenAsync_lanza_excepcion_si_fcmToken_o_deviceType_estan_vacios()
    {
        await using var contexto = CrearContexto();
        var logger = NullLogger<FirebaseNotificacionService>.Instance;
        var servicio = new FirebaseNotificacionService(contexto, logger);

        await Assert.ThrowsAsync<AppException>(() =>
            servicio.RegistrarTokenAsync(10, new RegistrarDispositivoDto { FcmToken = "", DeviceType = "android" }));

        await Assert.ThrowsAsync<AppException>(() =>
            servicio.RegistrarTokenAsync(10, new RegistrarDispositivoDto { FcmToken = "token_valido", DeviceType = "  " }));
    }

    [Fact]
    public async Task DesactivarTokenAsync_desactiva_el_token()
    {
        await using var contexto = CrearContexto();
        var token = new UserDeviceToken
        {
            UserId = 30,
            DeviceToken = "token_para_desactivar",
            DeviceType = "android",
            Activo = true
        };
        contexto.UserDeviceTokens.Add(token);
        await contexto.SaveChangesAsync();

        var logger = NullLogger<FirebaseNotificacionService>.Instance;
        var servicio = new FirebaseNotificacionService(contexto, logger);

        await servicio.DesactivarTokenAsync("token_para_desactivar");

        var tokenDesactivado = await contexto.UserDeviceTokens.FirstOrDefaultAsync(t => t.DeviceToken == "token_para_desactivar");
        Assert.NotNull(tokenDesactivado);
        Assert.False(tokenDesactivado.Activo);
        Assert.NotNull(tokenDesactivado.UpdatedAt);
    }

    [Fact]
    public async Task AsignarAsync_envia_notificacion_a_revisor_asignado()
    {
        await using var contexto = CrearContexto();

        var director = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Director", Email = "dir@uth.edu.mx" };
        var revisor = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Revisor", Email = "rev@uth.edu.mx" };
        var rolDirector = new RolEntidad { Id = 1, Nombre = "Director" };
        var rolRevisor = new RolEntidad { Id = 2, Nombre = "Revisor" };

        var periodo = new Periodo { Id = 10, Nombre = "Mayo - Agosto 2026", CicloEscolarId = 1 };
        var asignatura = new Asignatura { Id = 20, Nombre = "Desarrollo Web Integral", Clave = "DWI-01", Cuatrimestre = 9 };

        var planeacion = new PlaneacionDidactica
        {
            Id = 100,
            PublicId = Guid.NewGuid(),
            PeriodoId = 10,
            Periodo = periodo,
            AsignaturaId = 20,
            Asignatura = asignatura,
            Estado = EstadoPlaneacion.Borrador,
            Activo = true
        };

        contexto.AddRange(director, revisor, rolDirector, rolRevisor, periodo, asignatura, planeacion);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = director.Id, RolId = rolDirector.Id },
            new UsuarioRol { UsuarioId = revisor.Id, RolId = rolRevisor.Id }
        );
        await contexto.SaveChangesAsync();

        var fakeFirebase = new FakeFirebaseNotificacionService();
        var autorizacion = new AutorizacionService(contexto);
        var lifecycle = new PeriodoLifecycleService(contexto, new RelojAcademico(TimeProvider.System));
        var servicio = new AsignacionRevisorPlaneacionService(contexto, autorizacion, lifecycle, fakeFirebase, NullLogger<AsignacionRevisorPlaneacionService>.Instance);

        var resultado = await servicio.AsignarAsync(planeacion.PublicId, revisor.PublicId, director.Id);

        Assert.NotNull(resultado);
        Assert.Equal(revisor.PublicId, resultado.RevisorPublicId);
        Assert.True(fakeFirebase.NotificacionEnviada);
        Assert.Equal(revisor.Id, fakeFirebase.UltimoUsuarioIdNotificado);
        Assert.Contains("Desarrollo Web Integral", fakeFirebase.UltimoMensaje);
        Assert.Equal("ASIGNACION_REVISOR", fakeFirebase.UltimosDatos?["tipo"]);
    }

    [Fact]
    public async Task EnviarARevisionAsync_envia_notificacion_a_revisor_asignado()
    {
        await using var contexto = CrearContexto();

        var docente = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Docente", Email = "doc@uth.edu.mx" };
        var revisor = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Revisor", Email = "rev@uth.edu.mx" };
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        var rolRevisor = new RolEntidad { Id = 2, Nombre = "Revisor" };

        var periodo = new Periodo { Id = 10, Nombre = "Mayo - Agosto 2026", CicloEscolarId = 1 };
        var asignatura = new Asignatura { Id = 20, Nombre = "Estructuras de Datos", Clave = "ED-01", Cuatrimestre = 3 };

        var planeacion = new PlaneacionDidactica
        {
            Id = 100,
            PublicId = Guid.NewGuid(),
            PeriodoId = 10,
            Periodo = periodo,
            AsignaturaId = 20,
            Asignatura = asignatura,
            RevisorId = revisor.Id,
            Revisor = revisor,
            Estado = EstadoPlaneacion.Borrador,
            Activo = true
        };

        var carga = new CargaAcademica
        {
            Id = 50,
            DocenteId = docente.Id,
            Docente = docente,
            PeriodoId = 10,
            Periodo = periodo,
            AsignaturaId = 20,
            Asignatura = asignatura,
            GrupoId = 1,
            Activo = true
        };

        contexto.AddRange(docente, revisor, rolDocente, rolRevisor, periodo, asignatura, planeacion, carga);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id },
            new UsuarioRol { UsuarioId = revisor.Id, RolId = rolRevisor.Id }
        );
        await contexto.SaveChangesAsync();

        var fakeFirebase = new FakeFirebaseNotificacionService();
        var autorizacion = new AutorizacionService(contexto);
        var lifecycle = new PeriodoLifecycleService(contexto, new RelojAcademico(TimeProvider.System));
        var servicio = new EstadoPlaneacionService(contexto, autorizacion, lifecycle, fakeFirebase, NullLogger<EstadoPlaneacionService>.Instance);

        var resultado = await servicio.EnviarARevisionAsync(planeacion.PublicId, docente.Id);

        Assert.NotNull(resultado);
        Assert.Equal(EstadoPlaneacion.EnRevision, resultado.Estado);
        Assert.True(fakeFirebase.NotificacionEnviada);
        Assert.Equal(revisor.Id, fakeFirebase.UltimoUsuarioIdNotificado);
        Assert.Contains("Estructuras de Datos", fakeFirebase.UltimoMensaje);
        Assert.Equal("PLANEACION_ENVIADA_REVISION", fakeFirebase.UltimosDatos?["tipo"]);
    }

    [Fact]
    public async Task ResolverRevisionAsync_envia_notificacion_a_docentes_asignados()
    {
        await using var contexto = CrearContexto();

        var docente1 = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Docente 1", Email = "doc1@uth.edu.mx" };
        var docente2 = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Docente 2", Email = "doc2@uth.edu.mx" };
        var revisor = new Usuario { Id = 3, PublicId = Guid.NewGuid(), Nombre = "Revisor", Email = "rev@uth.edu.mx" };
        var rolRevisor = new RolEntidad { Id = 2, Nombre = "Revisor" };

        var periodo = new Periodo { Id = 10, Nombre = "Mayo - Agosto 2026", CicloEscolarId = 1 };
        var asignatura = new Asignatura { Id = 20, Nombre = "Bases de Datos", Clave = "BD-01", Cuatrimestre = 4 };

        var planeacion = new PlaneacionDidactica
        {
            Id = 100,
            PublicId = Guid.NewGuid(),
            PeriodoId = 10,
            Periodo = periodo,
            AsignaturaId = 20,
            Asignatura = asignatura,
            RevisorId = revisor.Id,
            Revisor = revisor,
            Estado = EstadoPlaneacion.EnRevision,
            Activo = true
        };

        var carga1 = new CargaAcademica
        {
            Id = 50,
            DocenteId = docente1.Id,
            PeriodoId = 10,
            AsignaturaId = 20,
            GrupoId = 1,
            Activo = true
        };
        var carga2 = new CargaAcademica
        {
            Id = 51,
            DocenteId = docente2.Id,
            PeriodoId = 10,
            AsignaturaId = 20,
            GrupoId = 2,
            Activo = true
        };

        contexto.AddRange(docente1, docente2, revisor, rolRevisor, periodo, asignatura, planeacion, carga1, carga2);
        contexto.UsuarioRoles.Add(new UsuarioRol { UsuarioId = revisor.Id, RolId = rolRevisor.Id });
        await contexto.SaveChangesAsync();

        var fakeFirebase = new FakeFirebaseNotificacionService();
        var autorizacion = new AutorizacionService(contexto);
        var lifecycle = new PeriodoLifecycleService(contexto, new RelojAcademico(TimeProvider.System));
        var servicio = new EstadoPlaneacionService(contexto, autorizacion, lifecycle, fakeFirebase, NullLogger<EstadoPlaneacionService>.Instance);

        var resultado = await servicio.ResolverRevisionAsync(planeacion.PublicId, revisor.Id, EstadoPlaneacion.Aprobada);

        Assert.NotNull(resultado);
        Assert.Equal(EstadoPlaneacion.Aprobada, resultado.Estado);
        Assert.True(fakeFirebase.NotificacionEnviada);
        Assert.Contains(docente1.Id, fakeFirebase.UltimosUsuariosIdsNotificados);
        Assert.Contains(docente2.Id, fakeFirebase.UltimosUsuariosIdsNotificados);
        Assert.Contains("aprobada", fakeFirebase.UltimoMensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("PLANEACION_APROBADA", fakeFirebase.UltimosDatos?["tipo"]);
    }

    [Fact]
    public async Task ComentariosCorreccionService_CrearAsync_envia_notificacion_a_contraparte()
    {
        await using var contexto = CrearContexto();

        var docente = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Ana", ApellidoPaterno = "Gómez", Email = "ana@uth.edu.mx" };
        var revisor = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Carlos", ApellidoPaterno = "López", Email = "carlos@uth.edu.mx" };
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        var rolRevisor = new RolEntidad { Id = 2, Nombre = "Revisor" };

        var periodo = new Periodo { Id = 10, Nombre = "Mayo - Agosto 2026", CicloEscolarId = 1 };
        var asignatura = new Asignatura { Id = 20, Nombre = "Programación Web", Clave = "PW-01", Cuatrimestre = 5 };

        var planeacion = new PlaneacionDidactica
        {
            Id = 100,
            PublicId = Guid.NewGuid(),
            PeriodoId = 10,
            Periodo = periodo,
            AsignaturaId = 20,
            Asignatura = asignatura,
            RevisorId = revisor.Id,
            Revisor = revisor,
            Estado = EstadoPlaneacion.CorreccionSolicitada,
            Activo = true
        };

        var carga = new CargaAcademica
        {
            Id = 50,
            DocenteId = docente.Id,
            Docente = docente,
            PeriodoId = 10,
            Periodo = periodo,
            AsignaturaId = 20,
            Asignatura = asignatura,
            GrupoId = 1,
            Activo = true
        };

        contexto.AddRange(docente, revisor, rolDocente, rolRevisor, periodo, asignatura, planeacion, carga);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id },
            new UsuarioRol { UsuarioId = revisor.Id, RolId = rolRevisor.Id }
        );
        await contexto.SaveChangesAsync();

        var fakeFirebase = new FakeFirebaseNotificacionService();
        var lifecycle = new PeriodoLifecycleService(contexto, new RelojAcademico(TimeProvider.System));
        var servicio = new ComentariosCorreccionService(contexto, lifecycle, fakeFirebase, NullLogger<ComentariosCorreccionService>.Instance);

        // Docente publica comentario -> Notifica al revisor
        var comentarioDocente = await servicio.CrearAsync(planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "Ya corregí la unidad 2." }, docente.Id);
        Assert.NotNull(comentarioDocente);
        Assert.True(fakeFirebase.NotificacionEnviada);
        Assert.Equal(revisor.Id, fakeFirebase.UltimoUsuarioIdNotificado);
        Assert.Contains("Ana Gómez", fakeFirebase.UltimoMensaje);
        Assert.Equal("NUEVO_COMENTARIO_CORRECCION", fakeFirebase.UltimosDatos?["tipo"]);

        // Revisor publica comentario -> Notifica al docente
        var comentarioRevisor = await servicio.CrearAsync(planeacion.PublicId, new CrearComentarioCorreccionDto { Mensaje = "Excelente, procederé a revisar." }, revisor.Id);
        Assert.NotNull(comentarioRevisor);
        Assert.True(fakeFirebase.NotificacionEnviada);
        Assert.Contains(docente.Id, fakeFirebase.UltimosUsuariosIdsNotificados);
        Assert.Contains("Carlos López", fakeFirebase.UltimoMensaje);
        Assert.Equal("NUEVO_COMENTARIO_CORRECCION", fakeFirebase.UltimosDatos?["tipo"]);
    }

    private sealed class FakeFirebaseNotificacionService : Interfaces.IFirebaseNotificacionService
    {
        public bool NotificacionEnviada { get; private set; }
        public long UltimoUsuarioIdNotificado { get; private set; }
        public List<long> UltimosUsuariosIdsNotificados { get; private set; } = [];
        public string UltimoTitulo { get; private set; } = string.Empty;
        public string UltimoMensaje { get; private set; } = string.Empty;
        public IReadOnlyDictionary<string, string>? UltimosDatos { get; private set; }

        public Task RegistrarTokenAsync(long usuarioId, RegistrarDispositivoDto dto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DesactivarTokenAsync(string fcmToken, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> SendNotificationAsync(long usuarioId, string titulo, string mensaje, IReadOnlyDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
        {
            NotificacionEnviada = true;
            UltimoUsuarioIdNotificado = usuarioId;
            UltimosUsuariosIdsNotificados = [usuarioId];
            UltimoTitulo = titulo;
            UltimoMensaje = mensaje;
            UltimosDatos = data;
            return Task.FromResult(true);
        }

        public Task<int> SendNotificationToUsersAsync(IEnumerable<long> usuariosIds, string titulo, string mensaje, IReadOnlyDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
        {
            var list = usuariosIds.ToList();
            NotificacionEnviada = true;
            UltimosUsuariosIdsNotificados = list;
            if (list.Count > 0)
                UltimoUsuarioIdNotificado = list[0];
            UltimoTitulo = titulo;
            UltimoMensaje = mensaje;
            UltimosDatos = data;
            return Task.FromResult(list.Count);
        }
    }
}
