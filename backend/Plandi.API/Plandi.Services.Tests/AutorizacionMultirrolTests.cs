using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services;
using Plandi.Services.Interfaces;
using Xunit;
using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services.Tests;

public sealed class AutorizacionMultirrolTests
{
    [Theory]
    [InlineData("Docente", RolAutorizacion.Docente, true)]
    [InlineData("Revisor", RolAutorizacion.Revisor, true)]
    [InlineData("Director", RolAutorizacion.Director, true)]
    [InlineData("Docente", RolAutorizacion.Revisor, false)]
    public void HasRole_reconoce_roles_individuales(string nombreRol, RolAutorizacion rol, bool esperado)
    {
        var servicio = CrearAutorizacion();
        Assert.Equal(esperado, servicio.HasRole(Principal(10, nombreRol), rol));
    }

    [Fact]
    public async Task Jwt_y_servicio_preservan_Docente_y_Revisor()
    {
        var usuario = UsuarioConRoles("Docente", "Revisor");
        var tokenService = new TokenService(CrearContexto(), ConfiguracionJwt());
        var (token, _) = await tokenService.GenerateAccessToken(usuario);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roles = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToHashSet();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, "test", ClaimTypes.NameIdentifier, ClaimTypes.Role));
        var autorizacion = CrearAutorizacion();

        Assert.Equal(new HashSet<string> { "Docente", "Revisor" }, roles);
        Assert.True(autorizacion.HasRole(principal, RolAutorizacion.Docente));
        Assert.True(autorizacion.HasRole(principal, RolAutorizacion.Revisor));
        Assert.False(autorizacion.HasRole(principal, RolAutorizacion.Director));
        Assert.Equal(10, autorizacion.ObtenerUsuarioId(principal));
    }

    [Fact]
    public async Task ExigirRolAsync_rechaza_usuario_sin_rol_requerido()
    {
        await using var contexto = CrearContexto();
        contexto.Usuarios.Add(new Usuario { Id = 10, Nombre = "Usuario", ApellidoPaterno = "Prueba", Email = "prueba@uth.edu.mx" });
        contexto.Roles.Add(new RolEntidad { Id = 1, Nombre = "Docente" });
        contexto.UsuarioRoles.Add(new UsuarioRol { UsuarioId = 10, RolId = 1 });
        await contexto.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new AutorizacionService(contexto).ExigirRolAsync(10, RolAutorizacion.Revisor));
    }

    [Fact]
    public async Task Docente_no_puede_consultar_planeaciones_de_otro_docente()
    {
        await using var contexto = CrearContexto();
        PrepararUsuarioConRol(contexto, 10, "Docente");
        PrepararUsuarioConRol(contexto, 20, "Docente");
        PrepararContextoAcademico(contexto);
        contexto.CargasAcademicas.Add(new CargaAcademica { Id = 1, PeriodoId = 1, GrupoId = 1, AsignaturaId = 1, DocenteId = 10 });
        contexto.CargasAcademicas.Add(new CargaAcademica { Id = 2, PeriodoId = 1, GrupoId = 2, AsignaturaId = 2, DocenteId = 20 });
        contexto.PlaneacionesDidacticas.AddRange(
            new PlaneacionDidactica { Id = 1, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 1 },
            new PlaneacionDidactica { Id = 2, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 2 });
        await contexto.SaveChangesAsync();

        var resultado = await new MisPlaneacionesDocenteService(contexto, new AutorizacionService(contexto)).ObtenerAsync(10);

        Assert.Single(resultado);
        Assert.Equal(1, contexto.PlaneacionesDidacticas.Single(p => p.PublicId == resultado[0].PublicId).Id);
    }

    [Fact]
    public async Task Revisor_no_puede_consultar_planeaciones_no_asignadas()
    {
        await using var contexto = CrearContexto();
        PrepararUsuarioConRol(contexto, 10, "Revisor");
        PrepararContextoAcademico(contexto);
        contexto.PlaneacionesDidacticas.AddRange(
            new PlaneacionDidactica { Id = 1, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 1, RevisorId = 10 },
            new PlaneacionDidactica { Id = 2, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 2, RevisorId = 99 });
        await contexto.SaveChangesAsync();

        var resultado = await new PlaneacionesRevisorService(contexto, new AutorizacionService(contexto)).ObtenerAsync(10);

        Assert.Single(resultado);
        Assert.Equal(1, contexto.PlaneacionesDidacticas.Single(p => p.PublicId == resultado[0].PublicId).Id);
    }

    [Fact]
    public async Task Docente_y_Revisor_puede_operar_en_ambos_contextos()
    {
        await using var contexto = CrearContexto();
        PrepararUsuarioConRol(contexto, 10, "Docente");
        contexto.Roles.Add(new RolEntidad { Id = 2, Nombre = "Revisor" });
        contexto.UsuarioRoles.Add(new UsuarioRol { UsuarioId = 10, RolId = 2 });
        PrepararContextoAcademico(contexto);
        contexto.CargasAcademicas.Add(new CargaAcademica { Id = 1, PeriodoId = 1, GrupoId = 1, AsignaturaId = 1, DocenteId = 10 });
        var planeacionDocente = new PlaneacionDidactica { Id = 1, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 1 };
        var planeacionRevision = new PlaneacionDidactica { Id = 2, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 2, RevisorId = 10 };
        contexto.PlaneacionesDidacticas.AddRange(planeacionDocente, planeacionRevision);
        await contexto.SaveChangesAsync();
        var autorizacion = new AutorizacionService(contexto);

        var comoDocente = await new MisPlaneacionesDocenteService(contexto, autorizacion).ObtenerAsync(10);
        var comoRevisor = await new PlaneacionesRevisorService(contexto, autorizacion).ObtenerAsync(10);

        Assert.Contains(comoDocente, p => p.PublicId == planeacionDocente.PublicId);
        Assert.DoesNotContain(comoDocente, p => p.PublicId == planeacionRevision.PublicId);
        Assert.Contains(comoRevisor, p => p.PublicId == planeacionRevision.PublicId);
    }

    [Fact]
    public async Task Director_puede_asignar_y_quitar_un_segundo_rol()
    {
        await using var contexto = CrearContexto();
        var director = new Usuario { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Directora", ApellidoPaterno = "Prueba", Email = "directora@uth.edu.mx" };
        var usuario = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Docente", ApellidoPaterno = "Prueba", Email = "docente@uth.edu.mx" };
        var rolDocente = new RolEntidad { Id = 1, PublicId = Guid.NewGuid(), Nombre = "Docente" };
        var rolRevisor = new RolEntidad { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Revisor" };
        var rolDirector = new RolEntidad { Id = 3, PublicId = Guid.NewGuid(), Nombre = "Director" };
        contexto.AddRange(director, usuario, rolDocente, rolRevisor, rolDirector);
        contexto.UsuarioRoles.AddRange(new UsuarioRol { UsuarioId = 1, RolId = 3 }, new UsuarioRol { UsuarioId = 2, RolId = 1 });
        await contexto.SaveChangesAsync();
        IGestionRolesUsuarioService gestion = new GestionRolesUsuarioService(contexto, new AutorizacionService(contexto));

        var conSegundoRol = await gestion.AsignarAsync(usuario.PublicId, rolRevisor.PublicId, director.Id);
        var sinSegundoRol = await gestion.QuitarAsync(usuario.PublicId, rolRevisor.PublicId, director.Id);

        Assert.Equal(new[] { "Docente", "Revisor" }, conSegundoRol.Roles.Select(rol => rol.Nombre));
        Assert.Equal(new[] { "Docente" }, sinSegundoRol.Roles.Select(rol => rol.Nombre));
    }

    private static AutorizacionService CrearAutorizacion() => new(CrearContexto());
    private static AppDbContext CrearContexto() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static ClaimsPrincipal Principal(long usuarioId, params string[] roles) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()), .. roles.Select(rol => new Claim(ClaimTypes.Role, rol))], "test", ClaimTypes.NameIdentifier, ClaimTypes.Role));
    private static Usuario UsuarioConRoles(params string[] roles) => new()
    {
        Id = 10, Nombre = "Usuario", ApellidoPaterno = "Prueba", Email = "prueba@uth.edu.mx",
        UsuarioRoles = roles.Select((nombre, indice) => new UsuarioRol { Rol = new RolEntidad { Id = indice + 1, Nombre = nombre } }).ToList()
    };

    private static void PrepararUsuarioConRol(AppDbContext contexto, long usuarioId, string nombreRol)
    {
        var rolId = nombreRol == "Docente" ? 1L : 2L;
        contexto.Usuarios.Add(new Usuario { Id = usuarioId, Nombre = $"Usuario{usuarioId}", ApellidoPaterno = "Prueba", Email = $"u{usuarioId}@uth.edu.mx" });
        if (!contexto.Roles.Local.Any(r => r.Id == rolId)) contexto.Roles.Add(new RolEntidad { Id = rolId, Nombre = nombreRol });
        contexto.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuarioId, RolId = rolId });
    }

    private static void PrepararContextoAcademico(AppDbContext contexto)
    {
        contexto.Periodos.Add(new Periodo { Id = 1, Nombre = "Periodo de prueba", CicloEscolarId = 1 });
        contexto.Asignaturas.AddRange(
            new Asignatura { Id = 1, Nombre = "Asignatura uno", Clave = "A-1", Cuatrimestre = 1 },
            new Asignatura { Id = 2, Nombre = "Asignatura dos", Clave = "A-2", Cuatrimestre = 1 });
    }
    private static IConfiguration ConfiguracionJwt() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:SecretKey"] = "clave-de-pruebas-con-longitud-suficiente-para-hs256",
        ["Jwt:Issuer"] = "Plandi.Tests", ["Jwt:Audience"] = "Plandi.Tests",
        ["Jwt:AccessTokenExpirationMinutes"] = "60", ["Jwt:RefreshTokenExpirationDays"] = "7"
    }).Build();
}
