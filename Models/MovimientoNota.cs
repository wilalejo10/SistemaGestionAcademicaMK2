using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_Academico.Models
{
    public class MovimientoNota
    {
        public int Id { get; set; }

        [Display(Name = "Nota")]
        public int? NotaId { get; set; }

        [ForeignKey("NotaId")]
        public virtual Nota? Nota { get; set; }

        [Display(Name = "Estudiante")]
        public int? EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante? Estudiante { get; set; }

        [Display(Name = "Materia")]
        public int? MateriaId { get; set; }

        [ForeignKey("MateriaId")]
        public virtual Materia? Materia { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Tipo")]
        public string TipoMovimiento { get; set; } = string.Empty; // Insercion | Modificacion | Eliminacion

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Valor Anterior")]
        public decimal? ValorAnterior { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Valor Nuevo")]
        public decimal? ValorNuevo { get; set; }

        [StringLength(50)]
        [Display(Name = "Período")]
        public string? Periodo { get; set; }

        [Display(Name = "Fecha")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Usuario que realizó")]
        public string? UsuarioAccion { get; set; }
    }
}
