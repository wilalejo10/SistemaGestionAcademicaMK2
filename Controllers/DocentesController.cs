using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;

namespace Sistema_Academico.Controllers
{
    public class DocentesController : Controller
    {
        private readonly AplicacionDbContext _context;

        public DocentesController(AplicacionDbContext context)
        {
            _context = context;
        }

        private bool EstaAutenticado() => HttpContext.Session.GetString("UsuarioRol") != null;
        private bool EsAdminOProfesor()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return rol == "Admin" || rol == "Profesor";
        }

        // GET: Docentes
        public async Task<IActionResult> Index(string? buscar)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            var query = _context.Docentes
                .Include(d => d.Materias)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(d => d.Nombre.Contains(buscar) ||
                                         d.Apellido.Contains(buscar) ||
                                         d.Correo.Contains(buscar));

            ViewBag.Buscar = buscar;
            return View(await query.OrderBy(d => d.Apellido).ThenBy(d => d.Nombre).ToListAsync());
        }

        // GET: Docentes/Crear
        public async Task<IActionResult> Crear()
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            ViewBag.Usuarios = new SelectList(
                await _context.Usuarios.Where(u => u.Rol == "Profesor").ToListAsync(),
                "Id", "NombreUsuario");
            return View(new Docente());
        }

        // POST: Docentes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Docente docente)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");

            ModelState.Remove("Materias");
            ModelState.Remove("Usuario");

            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(
                    await _context.Usuarios.Where(u => u.Rol == "Profesor").ToListAsync(),
                    "Id", "NombreUsuario");
                return View(docente);
            }

            _context.Docentes.Add(docente);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Docente registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Docentes/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var docente = await _context.Docentes.FindAsync(id);
            if (docente == null) return NotFound();
            ViewBag.Usuarios = new SelectList(
                await _context.Usuarios.Where(u => u.Rol == "Profesor").ToListAsync(),
                "Id", "NombreUsuario", docente.UsuarioId);
            return View(docente);
        }

        // POST: Docentes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Docente docente)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            if (id != docente.Id) return BadRequest();

            ModelState.Remove("Materias");
            ModelState.Remove("Usuario");

            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(
                    await _context.Usuarios.Where(u => u.Rol == "Profesor").ToListAsync(),
                    "Id", "NombreUsuario", docente.UsuarioId);
                return View(docente);
            }

            try
            {
                _context.Update(docente);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Docente actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Docentes.AnyAsync(d => d.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Docentes/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var docente = await _context.Docentes
                .Include(d => d.Materias)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (docente == null) return NotFound();
            return View(docente);
        }

        // POST: Docentes/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var docente = await _context.Docentes.FindAsync(id);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Docente eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Docentes/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");
            var docente = await _context.Docentes
                .Include(d => d.Materias)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (docente == null) return NotFound();
            return View(docente);
        }
    }
}
