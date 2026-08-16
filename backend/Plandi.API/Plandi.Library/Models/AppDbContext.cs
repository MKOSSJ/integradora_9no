using Plandi.Dto.Enums;
using Microsoft.EntityFrameworkCore;


namespace Plandi.Library.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    public DbSet<Academia> Academias => Set<Academia>();
    public DbSet<AcademiaUsuario> AcademiaUsuarios => Set<AcademiaUsuario>();

    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<CarreraAcademia> CarreraAcademias => Set<CarreraAcademia>();
    public DbSet<CicloEscolar> CiclosEscolares => Set<CicloEscolar>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<Asignatura> Asignaturas => Set<Asignatura>();

    public DbSet<CargaAcademica> CargasAcademicas => Set<CargaAcademica>();

    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<ProgramaAsignatura> ProgramasAsignatura => Set<ProgramaAsignatura>();

    public DbSet<PlaneacionDidactica> PlaneacionesDidacticas => Set<PlaneacionDidactica>();
    public DbSet<PlaneacionDocente> PlaneacionDocentes => Set<PlaneacionDocente>();
    public DbSet<PlaneacionGrupo> PlaneacionGrupos => Set<PlaneacionGrupo>();
    public DbSet<PlaneacionUnidad> PlaneacionUnidades => Set<PlaneacionUnidad>();
    public DbSet<PlaneacionActividad> PlaneacionActividades => Set<PlaneacionActividad>();
    public DbSet<PlaneacionObservacion> PlaneacionObservaciones => Set<PlaneacionObservacion>();

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatParticipante> ChatParticipantes => Set<ChatParticipante>();
    public DbSet<ChatMensaje> ChatMensajes => Set<ChatMensaje>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsuarios(modelBuilder);
        ConfigureAcademias(modelBuilder);
        ConfigureAcademico(modelBuilder);
        ConfigureDocumentos(modelBuilder);
        ConfigurePlaneaciones(modelBuilder);
        ConfigureChat(modelBuilder);
        ConfigureNotificaciones(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ApellidoPaterno).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ApellidoMaterno).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Telefono).HasMaxLength(30);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");

            entity.HasIndex(x => x.Nombre).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(250);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("usuario_roles");

            entity.HasKey(x => new { x.UsuarioId, x.RolId });

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAcademias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Academia>(entity =>
        {
            entity.ToTable("academias");

            entity.HasIndex(x => x.Nombre).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(300);
        });

        modelBuilder.Entity<AcademiaUsuario>(entity =>
        {
            entity.ToTable("academia_usuarios");

            entity.HasKey(x => new { x.AcademiaId, x.UsuarioId });

            entity.Property(x => x.RolEnAcademia)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(x => x.Academia)
                .WithMany(x => x.AcademiaUsuarios)
                .HasForeignKey(x => x.AcademiaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.AcademiaUsuarios)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAcademico(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrera>(entity =>
        {
            entity.ToTable("carreras");

            entity.HasIndex(x => x.Clave).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Clave).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Nivel).HasMaxLength(50);
        });

        modelBuilder.Entity<CicloEscolar>(entity =>
        {
            entity.ToTable("ciclos_escolares");

            entity.HasIndex(x => x.Nombre).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Periodo>(entity =>
        {
            entity.ToTable("periodos");

            entity.HasIndex(x => new { x.CicloEscolarId, x.Nombre }).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

            entity.HasOne(x => x.CicloEscolar)
                .WithMany(x => x.Periodos)
                .HasForeignKey(x => x.CicloEscolarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.ToTable("grupos");

            entity.HasIndex(x => new { x.PeriodoId, x.Nombre }).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(50).IsRequired();

            entity.HasOne(x => x.Carrera)
                .WithMany(x => x.Grupos)
                .HasForeignKey(x => x.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Periodo)
                .WithMany(x => x.Grupos)
                .HasForeignKey(x => x.PeriodoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Asignatura>(entity =>
        {
            entity.ToTable("asignaturas");

            entity.HasIndex(x => x.Clave).IsUnique();

            entity.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Clave).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Creditos).HasPrecision(5, 2);

            entity.HasOne(x => x.Academia)
                .WithMany(x => x.Asignaturas)
                .HasForeignKey(x => x.AcademiaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CarreraAcademia>(entity =>
        {
            entity.ToTable("carrera_academias");

            entity.HasKey(x => new { x.CarreraId, x.AcademiaId });

            entity.HasOne(x => x.Carrera)
                .WithMany(x => x.CarreraAcademias)
                .HasForeignKey(x => x.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Academia)
                .WithMany(x => x.CarreraAcademias)
                .HasForeignKey(x => x.AcademiaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CargaAcademica>(entity =>
        {
            entity.ToTable("carga_academica");

            entity.HasIndex(x => new
            {
                x.PeriodoId,
                x.GrupoId,
                x.AsignaturaId,
                x.DocenteId
            }).IsUnique();

            entity.HasOne(x => x.Periodo)
                .WithMany()
                .HasForeignKey(x => x.PeriodoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Grupo)
                .WithMany(x => x.CargasAcademicas)
                .HasForeignKey(x => x.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Asignatura)
                .WithMany(x => x.CargasAcademicas)
                .HasForeignKey(x => x.AsignaturaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Docente)
                .WithMany()
                .HasForeignKey(x => x.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Revisor)
                .WithMany()
                .HasForeignKey(x => x.RevisorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Academia)
                .WithMany()
                .HasForeignKey(x => x.AcademiaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDocumentos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Documento>(entity =>
        {
            entity.ToTable("documentos");

            entity.HasIndex(x => x.HashSha256);
            entity.HasIndex(x => x.TipoDocumento);
            entity.HasIndex(x => x.Estado);

            entity.Property(x => x.TipoDocumento)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(x => x.Estado)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
            entity.Property(x => x.NombreOriginal).HasMaxLength(255).IsRequired();
            entity.Property(x => x.NombreGuardado).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Extension).HasMaxLength(20).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RutaStorage).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.HashSha256).HasMaxLength(64);

            entity.HasOne(x => x.SubidoPor)
                .WithMany()
                .HasForeignKey(x => x.SubidoPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProgramaAsignatura>(entity =>
        {
            entity.ToTable("programas_asignatura");

            entity.HasIndex(x => x.AsignaturaId);
            entity.HasIndex(x => x.NombreAsignatura);
            entity.HasIndex(x => x.ClaveAsignatura);

            entity.Property(x => x.NombreAsignatura).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ClaveAsignatura).HasMaxLength(50);
            entity.Property(x => x.Carrera).HasMaxLength(200);
            entity.Property(x => x.Competencia).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Proposito).HasColumnType("nvarchar(max)");
            entity.Property(x => x.TextoExtraido).HasColumnType("nvarchar(max)");
            entity.Property(x => x.JsonExtraido).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Creditos).HasPrecision(5, 2);

            entity.HasOne(x => x.Documento)
                .WithOne(x => x.ProgramaAsignatura)
                .HasForeignKey<ProgramaAsignatura>(x => x.DocumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Asignatura)
                .WithMany(x => x.ProgramasAsignatura)
                .HasForeignKey(x => x.AsignaturaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Academia)
                .WithMany()
                .HasForeignKey(x => x.AcademiaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UltimaModificacionPor)
                .WithMany()
                .HasForeignKey(x => x.UltimaModificacionPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePlaneaciones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaneacionDidactica>(entity =>
        {
            entity.ToTable("planeaciones_didacticas");

            entity.HasIndex(x => new { x.PeriodoId, x.AsignaturaId }).IsUnique();
            entity.HasIndex(x => x.Estado);

            entity.Property(x => x.Titulo).HasMaxLength(250).IsRequired();

            entity.Property(x => x.Estado)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(x => x.Periodo)
                .WithMany()
                .HasForeignKey(x => x.PeriodoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Asignatura)
                .WithMany(x => x.PlaneacionesDidacticas)
                .HasForeignKey(x => x.AsignaturaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Academia)
                .WithMany()
                .HasForeignKey(x => x.AcademiaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ProgramaAsignatura)
                .WithMany()
                .HasForeignKey(x => x.ProgramaAsignaturaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Revisor)
                .WithMany()
                .HasForeignKey(x => x.RevisorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UltimaModificacionPor)
                .WithMany()
                .HasForeignKey(x => x.UltimaModificacionPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaneacionDocente>(entity =>
        {
            entity.ToTable("planeacion_docentes");

            entity.HasKey(x => new { x.PlaneacionDidacticaId, x.DocenteId });

            entity.HasOne(x => x.PlaneacionDidactica)
                .WithMany(x => x.PlaneacionDocentes)
                .HasForeignKey(x => x.PlaneacionDidacticaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Docente)
                .WithMany()
                .HasForeignKey(x => x.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaneacionGrupo>(entity =>
        {
            entity.ToTable("planeacion_grupos");

            entity.HasKey(x => new { x.PlaneacionDidacticaId, x.GrupoId });

            entity.HasOne(x => x.PlaneacionDidactica)
                .WithMany(x => x.PlaneacionGrupos)
                .HasForeignKey(x => x.PlaneacionDidacticaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Grupo)
                .WithMany(x => x.PlaneacionGrupos)
                .HasForeignKey(x => x.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaneacionUnidad>(entity =>
        {
            entity.ToTable("planeacion_unidades");

            entity.HasIndex(x => new { x.PlaneacionDidacticaId, x.Orden }).IsUnique();

            entity.Property(x => x.Numero).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(250).IsRequired();
            entity.Property(x => x.ResultadoAprendizaje).HasColumnType("nvarchar(max)");

            entity.HasOne(x => x.PlaneacionDidactica)
                .WithMany(x => x.Unidades)
                .HasForeignKey(x => x.PlaneacionDidacticaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UltimaModificacionPor)
                .WithMany()
                .HasForeignKey(x => x.UltimaModificacionPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaneacionActividad>(entity =>
        {
            entity.ToTable("planeacion_actividades");

            entity.HasIndex(x => new { x.PlaneacionUnidadId, x.Orden });

            entity.Property(x => x.TipoActividad).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Descripcion).HasColumnType("nvarchar(max)");
            entity.Property(x => x.EstrategiaEnsenanza).HasColumnType("nvarchar(max)");
            entity.Property(x => x.EstrategiaAprendizaje).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Evidencia).HasColumnType("nvarchar(max)");
            entity.Property(x => x.InstrumentoEvaluacion).HasMaxLength(200);
            entity.Property(x => x.PorcentajeEvaluacion).HasPrecision(5, 2);

            entity.HasOne(x => x.PlaneacionUnidad)
                .WithMany(x => x.Actividades)
                .HasForeignKey(x => x.PlaneacionUnidadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaneacionObservacion>(entity =>
        {
            entity.ToTable("planeacion_observaciones");

            entity.Property(x => x.Comentario).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Estado).HasMaxLength(50).IsRequired();

            entity.HasOne(x => x.PlaneacionDidactica)
                .WithMany()
                .HasForeignKey(x => x.PlaneacionDidacticaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PlaneacionUnidad)
                .WithMany()
                .HasForeignKey(x => x.PlaneacionUnidadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Revisor)
                .WithMany()
                .HasForeignKey(x => x.RevisorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureChat(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("chats");

            entity.HasIndex(x => x.PlaneacionDidacticaId).IsUnique();

            entity.Property(x => x.Titulo).HasMaxLength(250).IsRequired();

            entity.HasOne(x => x.PlaneacionDidactica)
                .WithMany(x => x.Chats)
                .HasForeignKey(x => x.PlaneacionDidacticaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatParticipante>(entity =>
        {
            entity.ToTable("chat_participantes");

            entity.HasKey(x => new { x.ChatId, x.UsuarioId });

            entity.Property(x => x.RolEnChat).HasMaxLength(50).IsRequired();

            entity.HasOne(x => x.Chat)
                .WithMany(x => x.Participantes)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMensaje>(entity =>
        {
            entity.ToTable("chat_mensajes");

            entity.HasIndex(x => x.ChatId);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.Mensaje).HasColumnType("nvarchar(max)");
            entity.Property(x => x.TipoMensaje).HasMaxLength(50).IsRequired();

            entity.HasOne(x => x.Chat)
                .WithMany(x => x.Mensajes)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureNotificaciones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("notificaciones");

            entity.HasIndex(x => new { x.UsuarioId, x.Leida, x.CreatedAt });
            entity.HasIndex(x => x.PlaneacionDidacticaId);

            entity.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Mensaje).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Leida).HasDefaultValue(false);

            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PlaneacionDidactica)
                .WithMany()
                .HasForeignKey(x => x.PlaneacionDidacticaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        DataSeeder.Seed(modelBuilder);
    }
}
