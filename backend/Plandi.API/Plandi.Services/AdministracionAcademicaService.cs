using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class AdministracionAcademicaService(AppDbContext context, IPeriodoLifecycleService lifecycle) : IAdministracionAcademicaService
{
    public async Task<PagedResult<AdminUsuarioDto>> UsuariosAsync(AdminConsultaDto f, CancellationToken ct = default)
    {
        var q = context.Usuarios.AsNoTracking().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.Trim();
            q = q.Where(x => x.Nombre.Contains(s) || x.ApellidoPaterno.Contains(s) || (x.ApellidoMaterno != null && x.ApellidoMaterno.Contains(s)) || (x.Email != null && x.Email.Contains(s)));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.ApellidoPaterno).ThenBy(x => x.Nombre).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize)
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.AcademiaUsuarios).ThenInclude(x => x.Academia)
            .AsSplitQuery().ToListAsync(ct);
        var ids = items.Select(x => x.Id).ToList();
        var cargas = await context.CargasAcademicas.AsNoTracking().Where(x => ids.Contains(x.DocenteId) && x.DeletedAt == null)
            .GroupBy(x => x.DocenteId).Select(x => new { Id = x.Key, Total = x.Count() }).ToDictionaryAsync(x => x.Id, x => x.Total, ct);
        return Page(items.Select(x => MapUsuario(x, cargas.GetValueOrDefault(x.Id))).ToList(), f, total);
    }

    public async Task<AdminUsuarioDto> UsuarioAsync(Guid id, CancellationToken ct = default)
    {
        var x = await context.Usuarios.AsNoTracking().Include(u => u.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(u => u.AcademiaUsuarios).ThenInclude(x => x.Academia)
            .SingleOrDefaultAsync(x => x.PublicId == id && x.DeletedAt == null, ct) ?? throw new NotFoundException("El usuario no existe.");
        var total = await context.CargasAcademicas.CountAsync(c => c.DocenteId == x.Id && c.DeletedAt == null, ct);
        return MapUsuario(x, total);
    }

    public async Task<PagedResult<AdminGrupoDto>> GruposAsync(AdminConsultaDto f, CancellationToken ct = default)
    {
        var q = context.Grupos.AsNoTracking().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search)) { var s = f.Search.Trim(); q = q.Where(x => x.Nombre.Contains(s) || x.Carrera.Nombre.Contains(s) || x.Periodo.Nombre.Contains(s)); }
        var total = await q.CountAsync(ct);
        var items = await GrupoQuery(q).OrderBy(x => x.Periodo.FechaInicio).ThenBy(x => x.Nombre).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        var planes = await PlaneacionesParaCargas(items.SelectMany(x => x.CargasAcademicas), ct);
        return Page(items.Select(x => MapGrupo(x, planes)).ToList(), f, total);
    }

    public async Task<AdminGrupoDto> GrupoAsync(Guid id, CancellationToken ct = default)
    {
        var x = await GrupoQuery(context.Grupos.AsNoTracking().Where(x => x.PublicId == id && x.DeletedAt == null)).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("El grupo no existe.");
        return MapGrupo(x, await PlaneacionesParaCargas(x.CargasAcademicas, ct));
    }

    public async Task<PagedResult<AdminAsignaturaDto>> AsignaturasAsync(AdminConsultaDto f, CancellationToken ct = default)
    {
        var q = context.Asignaturas.AsNoTracking().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search)) { var s = f.Search.Trim(); q = q.Where(x => x.Nombre.Contains(s) || x.Clave.Contains(s)); }
        var total = await q.CountAsync(ct);
        var items = await AsignaturaQuery(q).OrderBy(x => x.Nombre).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        var planes = await PlaneacionesParaCargas(items.SelectMany(x => x.CargasAcademicas), ct);
        return Page(items.Select(x => MapAsignatura(x, planes)).ToList(), f, total);
    }

    public async Task<AdminAsignaturaDto> AsignaturaAsync(Guid id, CancellationToken ct = default)
    {
        var x = await AsignaturaQuery(context.Asignaturas.AsNoTracking().Where(x => x.PublicId == id && x.DeletedAt == null)).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("La asignatura no existe.");
        return MapAsignatura(x, await PlaneacionesParaCargas(x.CargasAcademicas, ct));
    }

    public async Task<PagedResult<AdminCicloDto>> CiclosAsync(AdminConsultaDto f, CancellationToken ct = default)
    {
        var q = context.CiclosEscolares.AsNoTracking().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search)) { var s = f.Search.Trim(); q = q.Where(x => x.Nombre.Contains(s)); }
        var total = await q.CountAsync(ct);
        var items = await q.Include(x => x.Periodos).OrderByDescending(x => x.FechaInicio).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        var periodIds = items.SelectMany(x => x.Periodos).Select(x => x.Id).ToList();
        var stats = await PeriodoStats(periodIds, ct);
        return Page(items.Select(x => MapCiclo(x, stats)).ToList(), f, total);
    }

    public async Task<AdminCicloDto> CicloAsync(Guid id, CancellationToken ct = default)
    {
        var x = await context.CiclosEscolares.AsNoTracking().Include(x => x.Periodos).SingleOrDefaultAsync(x => x.PublicId == id && x.DeletedAt == null, ct) ?? throw new NotFoundException("El ciclo escolar no existe.");
        return MapCiclo(x, await PeriodoStats(x.Periodos.Select(p => p.Id), ct));
    }

    public async Task<PagedResult<AdminPeriodoDto>> PeriodosAsync(AdminConsultaDto f, CancellationToken ct = default)
    {
        var q = context.Periodos.AsNoTracking().Include(x => x.CicloEscolar).Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search)) { var s = f.Search.Trim(); q = q.Where(x => x.Nombre.Contains(s) || x.CicloEscolar.Nombre.Contains(s)); }
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.FechaInicio).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        var stats = await PeriodoStats(items.Select(x => x.Id), ct);
        return Page(items.Select(x => MapPeriodo(x, stats.GetValueOrDefault(x.Id))).ToList(), f, total);
    }

    public async Task<AdminPeriodoDto> PeriodoAsync(Guid id, CancellationToken ct = default)
    {
        var x = await context.Periodos.AsNoTracking().Include(x => x.CicloEscolar).SingleOrDefaultAsync(x => x.PublicId == id && x.DeletedAt == null, ct) ?? throw new NotFoundException("El periodo no existe.");
        return MapPeriodo(x, (await PeriodoStats([x.Id], ct)).GetValueOrDefault(x.Id));
    }

    public async Task<PagedResult<AdminCargaResumenDto>> CargasAsync(AdminConsultaDto f, CancellationToken ct = default)
    {
        var q = context.CargasAcademicas.AsNoTracking().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.Trim();
            q = q.Where(x => x.Docente.Nombre.Contains(s) || x.Docente.ApellidoPaterno.Contains(s) || x.Asignatura.Nombre.Contains(s) || x.Asignatura.Clave.Contains(s) || x.Grupo.Nombre.Contains(s) || x.Periodo.Nombre.Contains(s));
        }
        var total = await q.CountAsync(ct);
        var items = await CargaQuery(q).OrderByDescending(x => x.Periodo.FechaInicio).ThenBy(x => x.Docente.ApellidoPaterno).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        var planes = await PlaneacionesParaCargas(items, ct);
        return Page(items.Select(x => MapCarga(x, planes)).ToList(), f, total);
    }

    public async Task<AdminCargaResumenDto> CargaAsync(Guid id, CancellationToken ct = default)
    {
        var x = await CargaQuery(context.CargasAcademicas.AsNoTracking().Where(x => x.PublicId == id && x.DeletedAt == null)).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("La carga académica no existe.");
        return MapCarga(x, await PlaneacionesParaCargas([x], ct));
    }

    private static IQueryable<Grupo> GrupoQuery(IQueryable<Grupo> q) => q.Include(x => x.Carrera).Include(x => x.Periodo).ThenInclude(x => x.CicloEscolar)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Asignatura)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Docente)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Revisor)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Academia).AsSplitQuery();

    private static IQueryable<Asignatura> AsignaturaQuery(IQueryable<Asignatura> q) => q.Include(x => x.Academia).Include(x => x.ProgramasAsignatura.Where(p => p.DeletedAt == null))
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Periodo).ThenInclude(x => x.CicloEscolar)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Grupo).ThenInclude(x => x.Carrera)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Docente)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Revisor)
        .Include(x => x.CargasAcademicas.Where(c => c.DeletedAt == null)).ThenInclude(x => x.Academia).AsSplitQuery();

    private static IQueryable<CargaAcademica> CargaQuery(IQueryable<CargaAcademica> q) => q.Include(x => x.Periodo).ThenInclude(x => x.CicloEscolar)
        .Include(x => x.Grupo).ThenInclude(x => x.Carrera).Include(x => x.Asignatura).Include(x => x.Docente).Include(x => x.Revisor).Include(x => x.Academia);

    private async Task<Dictionary<(long, long), PlaneacionDidactica>> PlaneacionesParaCargas(IEnumerable<CargaAcademica> cargas, CancellationToken ct)
    {
        var pairs = cargas.Select(x => (x.PeriodoId, x.AsignaturaId)).Distinct().ToList();
        var periodoIds = pairs.Select(x => x.PeriodoId).Distinct().ToList();
        var asignaturaIds = pairs.Select(x => x.AsignaturaId).Distinct().ToList();
        if (pairs.Count == 0) return [];
        var planes = await context.PlaneacionesDidacticas.AsNoTracking().Where(x => x.DeletedAt == null && periodoIds.Contains(x.PeriodoId) && asignaturaIds.Contains(x.AsignaturaId)).ToListAsync(ct);
        return planes.Where(x => pairs.Contains((x.PeriodoId, x.AsignaturaId))).ToDictionary(x => (x.PeriodoId, x.AsignaturaId));
    }

    private sealed record PeriodoConteos(int Grupos, int Cargas, int Planeaciones);
    private async Task<Dictionary<long, PeriodoConteos>> PeriodoStats(IEnumerable<long> ids, CancellationToken ct)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return [];
        var grupos = await context.Grupos.AsNoTracking().Where(x => list.Contains(x.PeriodoId) && x.DeletedAt == null).GroupBy(x => x.PeriodoId).Select(x => new { x.Key, N = x.Count() }).ToDictionaryAsync(x => x.Key, x => x.N, ct);
        var cargas = await context.CargasAcademicas.AsNoTracking().Where(x => list.Contains(x.PeriodoId) && x.DeletedAt == null).GroupBy(x => x.PeriodoId).Select(x => new { x.Key, N = x.Count() }).ToDictionaryAsync(x => x.Key, x => x.N, ct);
        var planes = await context.PlaneacionesDidacticas.AsNoTracking().Where(x => list.Contains(x.PeriodoId) && x.DeletedAt == null).GroupBy(x => x.PeriodoId).Select(x => new { x.Key, N = x.Count() }).ToDictionaryAsync(x => x.Key, x => x.N, ct);
        return list.ToDictionary(x => x, x => new PeriodoConteos(grupos.GetValueOrDefault(x), cargas.GetValueOrDefault(x), planes.GetValueOrDefault(x)));
    }

    private AdminUsuarioDto MapUsuario(Usuario x, int total) => new()
    {
        PublicId = x.PublicId, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno,
        NombreCompleto = Nombre(x), Correo = x.Email, Activo = x.Activo,
        Roles = x.UsuarioRoles.Where(r => r.Rol.Activo && r.Rol.DeletedAt == null).Select(r => r.Rol.Nombre).Order().ToList(),
        Academias = x.AcademiaUsuarios.Where(a => a.Activo && a.Academia.DeletedAt == null).Select(a => Ref(a.Academia)).ToList(), TotalCargasAcademicas = total
    };

    private AdminGrupoDto MapGrupo(Grupo x, Dictionary<(long, long), PlaneacionDidactica> planes) => new()
    {
        PublicId = x.PublicId, Nombre = x.Nombre, Cuatrimestre = x.Cuatrimestre, Carrera = Ref(x.Carrera), Periodo = PeriodoRef(x.Periodo), Ciclo = Ref(x.Periodo.CicloEscolar),
        Activo = x.Activo, PermiteModificaciones = lifecycle.PermiteModificaciones(x.Periodo), Asignaciones = x.CargasAcademicas.Select(c => MapCarga(c, planes)).ToList()
    };

    private AdminAsignaturaDto MapAsignatura(Asignatura x, Dictionary<(long, long), PlaneacionDidactica> planes) => new()
    {
        PublicId = x.PublicId, Nombre = x.Nombre, Clave = x.Clave, Cuatrimestre = x.Cuatrimestre, HorasTotales = x.HorasTotales, HorasSemana = x.HorasSemana, Creditos = x.Creditos, Activo = x.Activo,
        Academia = x.Academia is null ? null : Ref(x.Academia), ProgramasAsignatura = x.ProgramasAsignatura.Select(p => new AdminEntidadResumenDto { PublicId = p.PublicId, Nombre = p.NombreAsignatura, Clave = p.ClaveAsignatura }).ToList(),
        Imparticiones = x.CargasAcademicas.Select(c => MapCarga(c, planes)).ToList()
    };

    private AdminCicloDto MapCiclo(CicloEscolar x, Dictionary<long, PeriodoConteos> stats) => new()
    {
        PublicId = x.PublicId, Nombre = x.Nombre, FechaInicio = x.FechaInicio, FechaFin = x.FechaFin, Activo = x.Activo,
        Periodos = x.Periodos.Where(p => p.DeletedAt == null).OrderBy(p => p.FechaInicio).Select(p => MapPeriodo(p, stats.GetValueOrDefault(p.Id))).ToList()
    };

    private AdminPeriodoDto MapPeriodo(Periodo x, PeriodoConteos? s) => new()
    {
        PublicId = x.PublicId, Nombre = x.Nombre, FechaInicio = x.FechaInicio, FechaFin = x.FechaFin, Estado = x.Estado,
        EstadoEfectivo = lifecycle.ObtenerEstadoEfectivo(x), FechaCierre = x.FechaCierre, PermiteModificaciones = lifecycle.PermiteModificaciones(x), Ciclo = Ref(x.CicloEscolar),
        TotalGrupos = s?.Grupos ?? 0, TotalCargas = s?.Cargas ?? 0, TotalPlaneaciones = s?.Planeaciones ?? 0
    };

    private AdminCargaResumenDto MapCarga(CargaAcademica x, Dictionary<(long, long), PlaneacionDidactica> planes)
    {
        planes.TryGetValue((x.PeriodoId, x.AsignaturaId), out var p);
        return new AdminCargaResumenDto
        {
            PublicId = x.PublicId, Docente = Ref(x.Docente, Nombre(x.Docente)), Asignatura = Ref(x.Asignatura), Grupo = Ref(x.Grupo), Periodo = PeriodoRef(x.Periodo), Ciclo = Ref(x.Periodo.CicloEscolar),
            Programa = Ref(x.Grupo.Carrera), Academia = x.Academia is null ? null : Ref(x.Academia), Revisor = x.Revisor is null ? null : Ref(x.Revisor, Nombre(x.Revisor)),
            PlaneacionPublicId = p?.PublicId, EstadoPlaneacion = p?.Estado, HorasTotales = x.Asignatura.HorasTotales, HorasSemana = x.Asignatura.HorasSemana,
            Activa = x.Activo, PermiteModificaciones = x.Activo && lifecycle.PermiteModificaciones(x.Periodo)
        };
    }

    private AdminPeriodoResumenDto PeriodoRef(Periodo x) => new() { PublicId = x.PublicId, Nombre = x.Nombre, FechaInicio = x.FechaInicio, FechaFin = x.FechaFin, Estado = lifecycle.ObtenerEstadoEfectivo(x), PermiteModificaciones = lifecycle.PermiteModificaciones(x) };
    private static AdminEntidadResumenDto Ref(BaseEntity x, string nombre, string? clave = null) => new() { PublicId = x.PublicId, Nombre = nombre, Clave = clave };
    private static AdminEntidadResumenDto Ref(Usuario x, string nombre) => Ref((BaseEntity)x, nombre);
    private static AdminEntidadResumenDto Ref(Asignatura x) => Ref(x, x.Nombre, x.Clave);
    private static AdminEntidadResumenDto Ref(Grupo x) => Ref(x, x.Nombre);
    private static AdminEntidadResumenDto Ref(Carrera x) => Ref(x, x.Nombre, x.Clave);
    private static AdminEntidadResumenDto Ref(CicloEscolar x) => Ref(x, x.Nombre);
    private static AdminEntidadResumenDto Ref(Academia x) => Ref(x, x.Nombre);
    private static string Nombre(Usuario x) => string.Join(" ", new[] { x.Nombre, x.ApellidoPaterno, x.ApellidoMaterno }.Where(v => !string.IsNullOrWhiteSpace(v)));
    private static PagedResult<T> Page<T>(IReadOnlyList<T> items, AdminConsultaDto f, int total) => new() { Items = items, Page = f.Page, PageSize = f.PageSize, TotalItems = total };
}
