using Microsoft.EntityFrameworkCore;

namespace Sistema_Academico.Models
{
    public class AplicacionDbContext : DbContext
    {
        public AplicacionDbContext(DbContextOptions<AplicacionDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<MovimientoNota> MovimientosNotas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Contrasena).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Rol).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Correo).IsUnique();
            });

            // Docente
            modelBuilder.Entity<Docente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Especialidad).HasMaxLength(100);
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Estudiante
            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Materia
            modelBuilder.Entity<Materia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasOne(e => e.Docente)
                      .WithMany(d => d.Materias)
                      .HasForeignKey(e => e.DocenteId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Nota
            modelBuilder.Entity<Nota>()
                .HasOne(n => n.Estudiante)
                .WithMany(e => e.Notas)
                .HasForeignKey(n => n.EstudianteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Nota>()
                .HasOne(n => n.Materia)
                .WithMany(m => m.Notas)
                .HasForeignKey(n => n.MateriaId)
                .OnDelete(DeleteBehavior.NoAction);

            // MovimientoNota
            modelBuilder.Entity<MovimientoNota>()
                .HasOne(mv => mv.Nota)
                .WithMany()
                .HasForeignKey(mv => mv.NotaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MovimientoNota>()
                .HasOne(mv => mv.Estudiante)
                .WithMany()
                .HasForeignKey(mv => mv.EstudianteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MovimientoNota>()
                .HasOne(mv => mv.Materia)
                .WithMany()
                .HasForeignKey(mv => mv.MateriaId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
