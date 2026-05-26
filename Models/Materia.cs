using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_Academico.Models
{
    public class Materia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Materia")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Display(Name = "Créditos")]
        [Range(1, 10)]
        public int Creditos { get; set; } = 3;

        [Display(Name = "Docente")]
        public int? DocenteId { get; set; }

        [ForeignKey("DocenteId")]
        public virtual Docente? Docente { get; set; }

        public virtual ICollection<Nota> Notas { get; set; } = new List<Nota>();
    }
}
