using System.ComponentModel.DataAnnotations;

namespace Sistema_Academico.Models
{
    public class RegistroNotaViewModel
    {
        public int EstudianteId { get; set; }
        public int MateriaId { get; set; }

        [Required]
        [Range(0, 5)]
        [Display(Name = "Nota")]
        public decimal Valor { get; set; }

        [Required]
        [Display(Name = "Período")]
        public string Periodo { get; set; } = string.Empty;

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        public List<Estudiante> Estudiantes { get; set; } = new();
        public List<Materia> Materias { get; set; } = new();
    }
}
