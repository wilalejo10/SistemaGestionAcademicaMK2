namespace Sistema_Academico.Models
{
    public class DashboardViewModel
    {
        public int TotalEstudiantes { get; set; }
        public int TotalDocentes { get; set; }
        public int TotalMaterias { get; set; }
        public int TotalNotas { get; set; }
        public decimal PromedioGeneral { get; set; }
        public List<GraficoDataPoint> PromediosPorMateria { get; set; } = new();
        public List<MovimientoNota> UltimosMovimientos { get; set; } = new();
    }
}
