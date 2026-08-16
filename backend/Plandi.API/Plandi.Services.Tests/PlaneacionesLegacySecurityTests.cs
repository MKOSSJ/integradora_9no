using Microsoft.EntityFrameworkCore;
using Plandi.Dto;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Xunit;

namespace Plandi.Services.Tests;

public sealed class PlaneacionesLegacySecurityTests
{
    [Theory]
    [InlineData(EstadoPlaneacion.Aprobada)]
    [InlineData(EstadoPlaneacion.Rechazada)]
    [InlineData(EstadoPlaneacion.Finalizada)]
    public async Task Referencia_no_se_crea_en_planeacion_terminal(EstadoPlaneacion estado)
    {
        await using var contexto = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var planeacion = new PlaneacionDidactica
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            PeriodoId = 1,
            AsignaturaId = 1,
            Estado = estado
        };
        contexto.Periodos.Add(new Periodo { Id = 1, Nombre = "Periodo de prueba", CicloEscolarId = 1 });
        contexto.PlaneacionesDidacticas.Add(planeacion);
        await contexto.SaveChangesAsync();
        var servicio = new PlaneacionReferenciaService(contexto, null!);

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.CreateAsync(
            planeacion.PublicId,
            new CreatePlaneacionReferenciaDto { ReferenciaAPA = "Referencia", Orden = 1 }));
        Assert.Empty(contexto.PlaneacionReferencias);
    }
}
