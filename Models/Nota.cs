using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_Academico.Models
{
    public class Nota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El estudiante es obligatorio")]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante? Estudiante { get; set; }

        [Required(ErrorMessage = "La materia es obligatoria")]
        [Display(Name = "Materia")]
        public int MateriaId { get; set; }

        [ForeignKey("MateriaId")]
        public virtual Materia? Materia { get; set; }

        [Required(ErrorMessage = "El valor es obligatorio")]
        [Display(Name = "Nota")]
        [Range(0, 5, ErrorMessage = "La nota debe estar entre 0 y 5")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "El período es obligatorio")]
        [StringLength(20)]
        [Display(Name = "Período")]
        public string Periodo { get; set; } = string.Empty; // Ej: 2024-1

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(200)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
    }
}
