using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Xunit;
using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services.Tests;

public sealed class GestionDocentesPlantillaTests
{
    [Fact]
    public async Task Director_completa_credenciales_de_docente_importado()
    {
        await using var contexto = CrearContexto();
        var director = new Usuario { Id = 1, Nombre = "Directora", ApellidoPaterno = "Prueba", Email = "directora@uth.edu.mx" };
        var docente = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Docente", ApellidoPaterno = "Importado" };
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        var rolDirector = new RolEntidad { Id = 3, Nombre = "Director" };
        contexto.AddRange(director, docente, rolDocente, rolDirector);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = director.Id, RolId = rolDirector.Id },
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id });
        await contexto.SaveChangesAsync();
        var servicio = new GestionDocentesPlantillaService(contexto, new AutorizacionService(contexto));

        var resultado = await servicio.CompletarCredencialesAsync(
            docente.PublicId,
            new CompletarCredencialesDocenteDto { Email = " DOCENTE@UTH.EDU.MX ", Password = "Password123" },
            director.Id);

        Assert.Equal("docente@uth.edu.mx", resultado.Email);
        Assert.True(resultado.CredencialesCompletas);
        Assert.Equal(resultado.Email, docente.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", docente.PasswordHash));
    }

    [Fact]
    public async Task No_sobrescribe_credenciales_existentes()
    {
        await using var contexto = CrearContexto();
        var director = new Usuario { Id = 1, Nombre = "Directora", ApellidoPaterno = "Prueba", Email = "directora@uth.edu.mx" };
        var docente = new Usuario
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            Nombre = "Docente",
            ApellidoPaterno = "Activo",
            Email = "existente@uth.edu.mx",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Anterior123")
        };
        var hashAnterior = docente.PasswordHash;
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        var rolDirector = new RolEntidad { Id = 3, Nombre = "Director" };
        contexto.AddRange(director, docente, rolDocente, rolDirector);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = director.Id, RolId = rolDirector.Id },
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id });
        await contexto.SaveChangesAsync();
        var servicio = new GestionDocentesPlantillaService(contexto, new AutorizacionService(contexto));

        await Assert.ThrowsAsync<AppException>(() => servicio.CompletarCredencialesAsync(
            docente.PublicId,
            new CompletarCredencialesDocenteDto { Email = "nuevo@uth.edu.mx", Password = "Password123" },
            director.Id));

        Assert.Equal("existente@uth.edu.mx", docente.Email);
        Assert.Equal(hashAnterior, docente.PasswordHash);
    }

    [Fact]
    public async Task Rechaza_email_que_ya_pertenece_a_otro_usuario()
    {
        await using var contexto = CrearContexto();
        var director = new Usuario { Id = 1, Nombre = "Directora", ApellidoPaterno = "Prueba", Email = "directora@uth.edu.mx" };
        var docente = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Docente", ApellidoPaterno = "Importado" };
        var existente = new Usuario { Id = 3, Nombre = "Usuario", ApellidoPaterno = "Existente", Email = "ocupado@uth.edu.mx" };
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        var rolDirector = new RolEntidad { Id = 3, Nombre = "Director" };
        contexto.AddRange(director, docente, existente, rolDocente, rolDirector);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = director.Id, RolId = rolDirector.Id },
            new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id });
        await contexto.SaveChangesAsync();
        var servicio = new GestionDocentesPlantillaService(contexto, new AutorizacionService(contexto));

        await Assert.ThrowsAsync<AppException>(() => servicio.CompletarCredencialesAsync(
            docente.PublicId,
            new CompletarCredencialesDocenteDto { Email = "OCUPADO@UTH.EDU.MX", Password = "Password123" },
            director.Id));

        Assert.Null(docente.Email);
        Assert.Null(docente.PasswordHash);
    }

    [Fact]
    public async Task Usuario_sin_rol_Director_no_puede_completar_credenciales()
    {
        await using var contexto = CrearContexto();
        var solicitante = new Usuario { Id = 1, Nombre = "Docente", ApellidoPaterno = "Solicitante" };
        var importado = new Usuario { Id = 2, PublicId = Guid.NewGuid(), Nombre = "Docente", ApellidoPaterno = "Importado" };
        var rolDocente = new RolEntidad { Id = 1, Nombre = "Docente" };
        contexto.AddRange(solicitante, importado, rolDocente);
        contexto.UsuarioRoles.AddRange(
            new UsuarioRol { UsuarioId = solicitante.Id, RolId = rolDocente.Id },
            new UsuarioRol { UsuarioId = importado.Id, RolId = rolDocente.Id });
        await contexto.SaveChangesAsync();
        var servicio = new GestionDocentesPlantillaService(contexto, new AutorizacionService(contexto));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.CompletarCredencialesAsync(
            importado.PublicId,
            new CompletarCredencialesDocenteDto { Email = "importado@uth.edu.mx", Password = "Password123" },
            solicitante.Id));

        Assert.Null(importado.Email);
        Assert.Null(importado.PasswordHash);
    }

    private static AppDbContext CrearContexto() => new(
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
