namespace Sistema_Academico.Models
{
    public class ReporteNotasViewModel
    {
        public string? EstudianteNombre { get; set; }
        public string? MateriaNombre { get; set; }
        public string? Periodo { get; set; }
        public decimal? NotaMin { get; set; }
        public decimal? NotaMax { get; set; }
        public List<Nota> Notas { get; set; } = new();
        public List<Estudiante> EstudiantesLista { get; set; } = new();
        public List<Materia> MateriasLista { get; set; } = new();
    }
}
