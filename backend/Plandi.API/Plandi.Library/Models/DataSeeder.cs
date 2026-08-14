using Microsoft.EntityFrameworkCore;

namespace Plandi.Library.Models;

/// <summary>
/// Datos mínimos necesarios para iniciar una base nueva. Las cuentas, programas,
/// cargas académicas y planeaciones se crean mediante los flujos de la API.
/// </summary>
public static class DataSeeder
{
    private static readonly DateTime FechaBase = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>().HasData(
            new Rol
            {
                Id = 1,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Nombre = "Docente",
                Descripcion = "Docente responsable de sus planeaciones.",
                CreatedAt = FechaBase
            },
            new Rol
            {
                Id = 2,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Nombre = "Revisor",
                Descripcion = "Usuario que revisa planeaciones asignadas.",
                CreatedAt = FechaBase
            },
            new Rol
            {
                Id = 3,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Nombre = "Director",
                Descripcion = "Usuario que administra asignaciones y roles.",
                CreatedAt = FechaBase
            });
    }
}
