using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;

namespace Sistema_Academico.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly AplicacionDbContext _context;

        public EstudiantesController(AplicacionDbContext context)
        {
            _context = context;
        }

        private bool EstaAutenticado() =>
            HttpContext.Session.GetString("UsuarioRol") != null;

        private bool EsAdminOProfesor()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return rol == "Admin" || rol == "Profesor";
        }

        // GET: Estudiantes
        public async Task<IActionResult> Index(string? buscar)
        {
            if (!EstaAutenticado())
                return RedirectToAction("Login", "Account");

            var query = _context.Estudiantes
                .Include(e => e.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(e =>
                    e.Nombre.Contains(buscar) ||
                    e.Apellido.Contains(buscar) ||
                    e.Codigo.Contains(buscar));

            ViewBag.Buscar = buscar;
            return View(await query
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync());
        }

        // GET: Estudiantes/Crear
        public async Task<IActionResult> Crear()
        {
            if (!EsAdminOProfesor())
                return RedirectToAction("Index", "Home");

            ViewBag.UsuarioId = new SelectList(
                await _context.Usuarios
                    .Where(u => u.Rol == "Estudiante")
                    .ToListAsync(),
                "Id", "NombreUsuario");

            return View(new Estudiante());
        }

        // POST: Estudiantes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Estudiante estudiante)
        {
            if (!EsAdminOProfesor())
                return RedirectToAction("Index", "Home");

            ModelState.Remove("Usuario");
            ModelState.Remove("Notas");

            if (await _context.Estudiantes
                    .AnyAsync(e => e.Codigo == estudiante.Codigo))
                ModelState.AddModelError("Codigo", "El código ya existe.");

            if (!ModelState.IsValid)
            {
                ViewBag.UsuarioId = new SelectList(
                    await _context.Usuarios
                        .Where(u => u.Rol == "Estudiante")
                        .ToListAsync(),
                    "Id", "NombreUsuario", estudiante.UsuarioId);
                return View(estudiante);
            }

            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Estudiante registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Estudiantes/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdminOProfesor())
                return RedirectToAction("Index", "Home");

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();

            ViewBag.UsuarioId = new SelectList(
                await _context.Usuarios
                    .Where(u => u.Rol == "Estudiante")
                    .ToListAsync(),
                "Id", "NombreUsuario", estudiante.UsuarioId);

            return View(estudiante);
        }

        // POST: Estudiantes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Estudiante estudiante)
        {
            if (!EsAdminOProfesor())
                return RedirectToAction("Index", "Home");

            if (id != estudiante.Id) return BadRequest();

            ModelState.Remove("Usuario");
            ModelState.Remove("Notas");

            if (!ModelState.IsValid)
            {
                ViewBag.UsuarioId = new SelectList(
                    await _context.Usuarios
                        .Where(u => u.Rol == "Estudiante")
                        .ToListAsync(),
                    "Id", "NombreUsuario", estudiante.UsuarioId);
                return View(estudiante);
            }

            try
            {
                _context.Update(estudiante);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Estudiante actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Estudiantes.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Estudiantes/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdminOProfesor())
                return RedirectToAction("Index", "Home");

            var estudiante = await _context.Estudiantes
                .Include(e => e.Notas)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (estudiante == null) return NotFound();
            return View(estudiante);
        }

        // POST: Estudiantes/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            if (!EsAdminOProfesor())
                return RedirectToAction("Index", "Home");

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante != null)
            {
                _context.Estudiantes.Remove(estudiante);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Estudiante eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Estudiantes/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            if (!EstaAutenticado())
                return RedirectToAction("Login", "Account");

            var estudiante = await _context.Estudiantes
                .Include(e => e.Usuario)
                .Include(e => e.Notas)
                    .ThenInclude(n => n.Materia)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (estudiante == null) return NotFound();
            return View(estudiante);
        }
    }
}
