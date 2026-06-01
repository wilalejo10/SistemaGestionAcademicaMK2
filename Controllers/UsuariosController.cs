using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;

namespace Sistema_Academico.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AplicacionDbContext _context;

        public UsuariosController(AplicacionDbContext context)
        {
            _context = context;
        }

        private bool EsAdmin() => HttpContext.Session.GetString("UsuarioRol") == "Admin";
        private bool EstaAutenticado() => HttpContext.Session.GetString("UsuarioRol") != null;

        // GET: Usuarios
        public async Task<IActionResult> Index(string? buscar, string? rol)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            var query = _context.Usuarios.AsQueryable();
            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(u => u.NombreUsuario.Contains(buscar) || u.Correo.Contains(buscar));
            if (!string.IsNullOrEmpty(rol))
                query = query.Where(u => u.Rol == rol);

            ViewBag.Buscar = buscar;
            ViewBag.Rol = rol;
            return View(await query.OrderBy(u => u.NombreUsuario).ToListAsync());
        }

        // GET: Usuarios/Crear
        public IActionResult Crear()
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");
            return View(new Usuario());
        }

        // POST: Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");

            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya existe.");

            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
                ModelState.AddModelError("Correo", "El correo ya está registrado.");

            if (!ModelState.IsValid) return View(usuario);

            usuario.FechaCreacion = DateTime.Now;
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Usuario usuario)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");
            if (id != usuario.Id) return BadRequest();

            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.Id != id))
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya existe.");

            if (!ModelState.IsValid) return View(usuario);

            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Usuario actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Usuarios.AnyAsync(u => u.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Usuario eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }
    }
}
