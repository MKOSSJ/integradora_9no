using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services;
using Xunit;
using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services.Tests;

public sealed class EdicionSecuenciaDidacticaTests
{
    [Fact]
    public async Task Edicion_crea_etapas_vacias_y_elementos_independientes_por_fase()
    {
        await using var contexto = CrearContexto();
        var docente = new Usuario { Id = 10, Nombre = "Docente", ApellidoPaterno = "Prueba" };
        var rol = new RolEntidad { Id = 1, Nombre = "Docente" };
        var planeacion = new PlaneacionDidactica { Id = 1, PublicId = Guid.NewGuid(), PeriodoId = 1, AsignaturaId = 1, Estado = EstadoPlaneacion.Borrador };
        var unidades = Enumerable.Range(1, 4).Select(numero => new PlaneacionUnidad
        {
            Id = numero + 10, PublicId = Guid.NewGuid(), PlaneacionDidactica = planeacion,
            NumeroUnidad = numero, NombreUnidad = $"Unidad {numero}", Orden = numero
        }).ToList();
        planeacion.Unidades = unidades;
        contexto.AddRange(docente, rol, planeacion);
        contexto.UsuarioRoles.Add(new UsuarioRol { UsuarioId = docente.Id, RolId = rol.Id });
        contexto.Periodos.Add(new Periodo { Id = 1, Nombre = "Periodo", CicloEscolarId = 1 });
        contexto.Asignaturas.Add(new Asignatura { Id = 1, Nombre = "Asignatura", Clave = "A-1", Cuatrimestre = 1 });
        contexto.CargasAcademicas.Add(new CargaAcademica { Id = 1, PeriodoId = 1, GrupoId = 1, AsignaturaId = 1, DocenteId = docente.Id });
        await contexto.SaveChangesAsync();

        var servicio = new EdicionPlaneacionService(contexto, new AutorizacionService(contexto));
        var estructuraInicial = await servicio.ActualizarAsync(planeacion.PublicId, docente.Id, Solicitud(unidades));

        Assert.Equal(12, contexto.PlaneacionEtapasSecuencia.Count());
        Assert.All(estructuraInicial.Unidades, unidad =>
        {
            Assert.NotNull(unidad.Apertura);
            Assert.NotNull(unidad.Desarrollo);
            Assert.NotNull(unidad.Cierre);
            Assert.Empty(unidad.Apertura);
            Assert.Empty(unidad.Desarrollo);
            Assert.Empty(unidad.Cierre);
        });
        foreach (var unidad in unidades)
            Assert.Equal(new[] { FaseSecuencia.Apertura, FaseSecuencia.Desarrollo, FaseSecuencia.Cierre },
                contexto.PlaneacionEtapasSecuencia.Where(e => e.PlaneacionUnidadId == unidad.Id).OrderBy(e => e.Fase).Select(e => e.Fase));

        var conElementos = Solicitud(unidades);
        conElementos.Unidades[0].Apertura =
        [
            Elemento(MetodoTecnicaEnsenanzaAprendizaje.WebQuest, 1, ["Internet", "PC"]),
            Elemento(MetodoTecnicaEnsenanzaAprendizaje.TecnicaExpositiva, 2),
            Elemento(MetodoTecnicaEnsenanzaAprendizaje.Conceptual, 3)
        ];
        conElementos.Unidades[0].Desarrollo =
        [
            Elemento(MetodoTecnicaEnsenanzaAprendizaje.Taller, 1),
            Elemento(MetodoTecnicaEnsenanzaAprendizaje.EstudioDeCaso, 2)
        ];
        conElementos.Unidades[0].Cierre = [Elemento(MetodoTecnicaEnsenanzaAprendizaje.AnalisisDeDesempeno, 1)];
        var detalle = await servicio.ActualizarAsync(planeacion.PublicId, docente.Id, conElementos);

        var elementos = contexto.PlaneacionSecuencias.Where(s => s.PlaneacionUnidadId == unidades[0].Id).OrderBy(s => s.Fase).ThenBy(s => s.Orden).ToList();
        Assert.Equal(6, elementos.Count);
        Assert.Equal(3, elementos.Count(s => s.Fase == FaseSecuencia.Apertura));
        Assert.Equal(2, elementos.Count(s => s.Fase == FaseSecuencia.Desarrollo));
        Assert.Single(elementos, s => s.Fase == FaseSecuencia.Cierre);
        var webQuest = elementos.Single(s => s.MetodoTecnica == MetodoTecnicaEnsenanzaAprendizaje.WebQuest);
        Assert.Equal(2, contexto.PlaneacionSecuenciaRecursos.Count(r => r.PlaneacionSecuenciaId == webQuest.Id));
        Assert.Equal(3, detalle.Unidades.Single(u => u.PublicId == unidades[0].PublicId).Apertura!.Count);
    }

    private static PlaneacionEdicionDto Solicitud(IEnumerable<PlaneacionUnidad> unidades) => new()
    {
        Caratula = new CaratulaPlaneacionEdicionDto(),
        Unidades = unidades.Select(u => new UnidadPlaneacionEdicionDto
        {
            PublicId = u.PublicId, NumeroUnidad = u.NumeroUnidad, NombreUnidad = u.NombreUnidad, Orden = u.Orden,
            Apertura = [], Desarrollo = [], Cierre = []
        }).ToList()
    };

    private static SecuenciaPlaneacionEdicionDto Elemento(MetodoTecnicaEnsenanzaAprendizaje metodo, int orden, string[]? recursos = null) => new()
    {
        MetodoTecnica = metodo, Orden = orden, ActividadDocente = $"Docente {metodo}",
        ActividadEstudiante = $"Estudiante {metodo}", EvidenciaAprendizaje = $"Evidencia {metodo}",
        Recursos = recursos?.Select((nombre, indice) => new RecursoSecuenciaPlaneacionEdicionDto { Nombre = nombre, Orden = indice + 1 }).ToList()
    };

    private static AppDbContext CrearContexto() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options);
}
