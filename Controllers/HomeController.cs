using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;
using System.Diagnostics;

namespace Sistema_Academico.Controllers
{
    public class HomeController : Controller
    {
        private readonly AplicacionDbContext _context;

        public HomeController(AplicacionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UsuarioRol") == null)
                return RedirectToAction("Login", "Account");

            var notas = await _context.Notas
                .Include(n => n.Materia)
                .Include(n => n.Estudiante)
                .ToListAsync();

            var movimientos = await _context.MovimientosNotas
                .Include(m => m.Estudiante)
                .Include(m => m.Materia)
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(10)
                .ToListAsync();

            var promediosPorMateria = await _context.Materias
                .Select(m => new GraficoDataPoint
                {
                    Etiqueta = m.Nombre,
                    Promedio = m.Notas.Any() ? Math.Round(m.Notas.Average(n => n.Valor), 2) : 0,
                    TotalEstudiantes = m.Notas.Select(n => n.EstudianteId).Distinct().Count()
                })
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalEstudiantes = await _context.Estudiantes.CountAsync(),
                TotalDocentes = await _context.Docentes.CountAsync(),
                TotalMaterias = await _context.Materias.CountAsync(),
                TotalNotas = notas.Count,
                PromedioGeneral = notas.Any() ? Math.Round(notas.Average(n => n.Valor), 2) : 0,
                PromediosPorMateria = promediosPorMateria,
                UltimosMovimientos = movimientos
            };

            return View(vm);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
