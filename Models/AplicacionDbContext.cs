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
            modelBuilder.Entity<Nota>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Valor).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Periodo).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Estudiante)
                      .WithMany(es => es.Notas)
                      .HasForeignKey(e => e.EstudianteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Materia)
                      .WithMany(m => m.Notas)
                      .HasForeignKey(e => e.MateriaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MovimientoNota
            modelBuilder.Entity<MovimientoNota>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoMovimiento).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ValorAnterior).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ValorNuevo).HasColumnType("decimal(5,2)");
                entity.HasOne(e => e.Nota)
                      .WithMany()
                      .HasForeignKey(e => e.NotaId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Estudiante)
                      .WithMany()
                      .HasForeignKey(e => e.EstudianteId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Materia)
                      .WithMany()
                      .HasForeignKey(e => e.MateriaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed data - Admin por defecto
            modelBuilder.Entity<Usuario>().HasData(new Usuario
            {
                Id = 1,
                NombreUsuario = "admin",
                Correo = "admin@sistema.edu",
                // Password: Admin123! (BCrypt hash)
                Contrasena = "$2a$11$rBnkuqK/hYE9L2Vg3JTQ6OZ.O4pGFtEjUKHlLWz5bX7Xb9K8mR.Ky",
                Rol = "Admin",
                FechaCreacion = new DateTime(2024, 1, 1)
            });
        }
    }
}
