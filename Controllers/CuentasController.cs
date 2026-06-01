using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;

namespace Sistema_Academico.Controllers
{
    public class CuentasController : Controller
    {
        private readonly AplicacionDbContext _context;

        public CuentasController(AplicacionDbContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == model.NombreUsuario && u.Activo);

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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        private bool VerificarContrasena(string contrasenaIngresada, string hashGuardado)
        {
            
            if (hashGuardado.StartsWith("$2a$") || hashGuardado.StartsWith("$2b$"))
            {
                return false;
            }
            return contrasenaIngresada == hashGuardado;
        }

        public IActionResult CambiarContrasena()
        {
            if (HttpContext.Session.GetString("UsuarioRol") == null)
                return RedirectToAction("Login");
            return View();
        }
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