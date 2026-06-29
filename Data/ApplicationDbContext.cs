using Microsoft.EntityFrameworkCore;
using secuenciasAPI.Models;

namespace secuenciasAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<CarreraDocente> CarreraDocentes { get; set; }
        public DbSet<CarreraMateria> CarreraMaterias { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Periodo> Periodos { get; set; }
        public DbSet<Programa> Programas { get; set; }
        public DbSet<Secuencia> Secuencias { get; set; }
        public DbSet<SecuenciaDocente> SecuenciaDocentes { get; set; }
        public DbSet<SecuenciaGrupo> SecuenciaGrupos { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RevisionSecuencia> RevisionesSecuencia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CarreraDocente>().HasKey(cd => new { cd.CarreraId, cd.DocenteId });
            modelBuilder.Entity<CarreraMateria>().HasKey(cm => new { cm.CarreraId, cm.MateriaId });
            modelBuilder.Entity<SecuenciaDocente>().HasKey(sd => new { sd.SecuenciaId, sd.DocenteId });
            modelBuilder.Entity<SecuenciaGrupo>().HasKey(sg => new { sg.SecuenciaId, sg.GrupoId });
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}