namespace Sistema_Academico.Models
{
    public class GraficoViewModel
    {
        public string? MateriaSeleccionada { get; set; }
        public string? Periodo { get; set; }
        public List<Materia> MateriasLista { get; set; } = new();
        public List<GraficoDataPoint> Datos { get; set; } = new();

    }
}
