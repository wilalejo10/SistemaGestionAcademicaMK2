using Microsoft.AspNetCore.Mvc;
using Sistema_Academico.Models;
using Microsoft.EntityFrameworkCore;

namespace Sistema_Academico.Controllers
{
    public class AccountController : Controller
    {
        private readonly AplicacionDbContext _context;

        public AccountController(AplicacionDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == model.NombreUsuario);

            if (usuario == null || !VerificarContrasena(model.Contrasena, usuario.Contrasena))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(model);
            }

            HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreUsuario);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
            HttpContext.Session.SetString("UsuarioCorreo", usuario.Correo);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── Helpers ────────────────────────────────────────
        private bool VerificarContrasena(string contrasenaIngresada, string hashGuardado)
        {
            // Si el hash empieza con $2a$ es BCrypt, sino comparar directo (para seed simple)
            if (hashGuardado.StartsWith("$2a$") || hashGuardado.StartsWith("$2b$"))
            {
                // Necesitaría BCrypt.Net. Para simplificar, usamos SHA256 en producción real.
                // Por ahora comparamos texto plano para el seed de prueba.
                return false;
            }
            return contrasenaIngresada == hashGuardado;
        }

        // GET: /Account/CambiarContrasena
        public IActionResult CambiarContrasena()
        {
            if (HttpContext.Session.GetString("UsuarioRol") == null)
                return RedirectToAction("Login");
            return View();
        }

        // POST: /Account/CambiarContrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(string contrasenaActual, string nuevaContrasena, string confirmar)
        {
            if (HttpContext.Session.GetString("UsuarioRol") == null)
                return RedirectToAction("Login");

            if (nuevaContrasena != confirmar)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            var id = int.Parse(HttpContext.Session.GetString("UsuarioId")!);
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null || usuario.Contrasena != contrasenaActual)
            {
                ViewBag.Error = "Contraseña actual incorrecta.";
                return View();
            }

            usuario.Contrasena = nuevaContrasena;
            await _context.SaveChangesAsync();
            ViewBag.Success = "Contraseña actualizada correctamente.";
            return View();
        }
    }
}
