using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;

namespace Sistema_Academico.Controllers
{
    public class MateriasController : Controller
    {
        private readonly AplicacionDbContext _context;

        public MateriasController(AplicacionDbContext context)
        {
            _context = context;
        }

        private bool EstaAutenticado() => HttpContext.Session.GetString("UsuarioRol") != null;
        private bool EsAdminOProfesor()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return rol == "Admin" || rol == "Profesor";
        }

        // GET: Materias
        public async Task<IActionResult> Index(string? buscar)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            var query = _context.Materias.Include(m => m.Docente).AsQueryable();
            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(m => m.Nombre.Contains(buscar) || m.Codigo.Contains(buscar));

            ViewBag.Buscar = buscar;
            return View(await query.OrderBy(m => m.Nombre).ToListAsync());
        }

        // GET: Materias/Crear
        public async Task<IActionResult> Crear()
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            ViewBag.DocenteId = new SelectList(await _context.Docentes.OrderBy(d => d.Apellido).ToListAsync(), "Id", "NombreCompleto");
            return View(new Materia());
        }

        // POST: Materias/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Materia materia)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            ModelState.Remove("Docente");
            ModelState.Remove("Notas");

            if (await _context.Materias.AnyAsync(m => m.Codigo == materia.Codigo))
                ModelState.AddModelError("Codigo", "El código ya existe.");

            if (!ModelState.IsValid)
            {
                ViewBag.DocenteId = new SelectList(await _context.Docentes.OrderBy(d => d.Apellido).ToListAsync(), "Id", "NombreCompleto", materia.DocenteId);
                return View(materia);
            }

            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Materia creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Materias/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound();
            ViewBag.DocenteId = new SelectList(await _context.Docentes.OrderBy(d => d.Apellido).ToListAsync(), "Id", "NombreCompleto", materia.DocenteId);
            return View(materia);
        }

        // POST: Materias/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Materia materia)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            if (id != materia.Id) return BadRequest();
            ModelState.Remove("Docente");
            ModelState.Remove("Notas");

            if (!ModelState.IsValid)
            {
                ViewBag.DocenteId = new SelectList(await _context.Docentes.OrderBy(d => d.Apellido).ToListAsync(), "Id", "NombreCompleto", materia.DocenteId);
                return View(materia);
            }

            try
            {
                _context.Update(materia);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Materia actualizada.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Materias.AnyAsync(m => m.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Materias/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var materia = await _context.Materias.Include(m => m.Docente).Include(m => m.Notas).FirstOrDefaultAsync(m => m.Id == id);
            if (materia == null) return NotFound();
            return View(materia);
        }

        // POST: Materias/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var materia = await _context.Materias.FindAsync(id);
            if (materia != null)
            {
                _context.Materias.Remove(materia);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Materia eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Materias/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");
            var materia = await _context.Materias
                .Include(m => m.Docente)
                .Include(m => m.Notas)
                    .ThenInclude(n => n.Estudiante)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (materia == null) return NotFound();
            return View(materia);
        }
    }
}
