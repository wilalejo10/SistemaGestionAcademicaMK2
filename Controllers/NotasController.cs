using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Academico.Models;

namespace Sistema_Academico.Controllers
{
    public class NotasController : Controller
    {
        private readonly AplicacionDbContext _context;

        public NotasController(AplicacionDbContext context)
        {
            _context = context;
        }

        private bool EstaAutenticado() => HttpContext.Session.GetString("UsuarioRol") != null;
        private bool EsAdminOProfesor()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return rol == "Admin" || rol == "Profesor";
        }
        private string UsuarioActual() => HttpContext.Session.GetString("UsuarioNombre") ?? "Sistema";

        //  CRUD REGISTRO_NOTAS

        // GET: Notas
        public async Task<IActionResult> Index(string? buscarEstudiante, int? materiaId, string? periodo)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            var rol = HttpContext.Session.GetString("UsuarioRol");
            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");

            var query = _context.Notas
                .Include(n => n.Estudiante)
                .Include(n => n.Materia)
                    .ThenInclude(m => m!.Docente)
                .AsQueryable();

            // Si es estudiante solo ve sus propias notas
            if (rol == "Estudiante")
            {
                var estudiante = await _context.Estudiantes
                    .Include(e => e.Usuario)
                    .FirstOrDefaultAsync(e => e.Usuario != null && e.Usuario.NombreUsuario == usuarioNombre);
                if (estudiante != null)
                    query = query.Where(n => n.EstudianteId == estudiante.Id);
                else
                    query = query.Where(n => false);
            }

            if (!string.IsNullOrEmpty(buscarEstudiante))
                query = query.Where(n => (n.Estudiante!.Nombre + " " + n.Estudiante.Apellido).Contains(buscarEstudiante) ||
                                          n.Estudiante.Codigo.Contains(buscarEstudiante));
            if (materiaId.HasValue)
                query = query.Where(n => n.MateriaId == materiaId);
            if (!string.IsNullOrEmpty(periodo))
                query = query.Where(n => n.Periodo == periodo);

            ViewBag.BuscarEstudiante = buscarEstudiante;
            ViewBag.MateriaId = materiaId;
            ViewBag.Periodo = periodo;
            ViewBag.Materias = new SelectList(await _context.Materias.OrderBy(m => m.Nombre).ToListAsync(), "Id", "Nombre", materiaId);
            ViewBag.Periodos = await _context.Notas.Select(n => n.Periodo).Distinct().OrderByDescending(p => p).ToListAsync();

            return View(await query.OrderByDescending(n => n.FechaRegistro).ToListAsync());
        }
        // GET: Notas/Registrar
        public async Task<IActionResult> Registrar()
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var vm = new RegistroNotaViewModel();
            await CargarSelectListsAsync(vm);
            return View(vm);
        }

        // POST: Notas/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroNotaViewModel vm)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");

            ModelState.Remove("Estudiantes");
            ModelState.Remove("Materias");

            if (!ModelState.IsValid)
            {
                await CargarSelectListsAsync(vm);
                return View(vm);
            }

            // Verificar si ya existe nota para ese estudiante/materia/periodo
            var existente = await _context.Notas.FirstOrDefaultAsync(n =>
                n.EstudianteId == vm.EstudianteId &&
                n.MateriaId == vm.MateriaId &&
                n.Periodo == vm.Periodo);

            if (existente != null)
            {
                ModelState.AddModelError("", "Ya existe una nota para este estudiante en esa materia y período. Use Modificar.");
                await CargarSelectListsAsync(vm);
                return View(vm);
            }

            var nota = new Nota
            {
                EstudianteId = vm.EstudianteId,
                MateriaId = vm.MateriaId,
                Valor = vm.Valor,
                Periodo = vm.Periodo,
                Observaciones = vm.Observaciones,
                FechaRegistro = DateTime.Now
            };
            _context.Notas.Add(nota);
            await _context.SaveChangesAsync();

            // Auditoría
            _context.MovimientosNotas.Add(new MovimientoNota
            {
                NotaId = nota.Id,
                EstudianteId = nota.EstudianteId,
                MateriaId = nota.MateriaId,
                TipoMovimiento = "Insercion",
                ValorNuevo = nota.Valor,
                Periodo = nota.Periodo,
                UsuarioAccion = UsuarioActual(),
                FechaMovimiento = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Nota {nota.Valor} registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // CREAR
        public async Task<IActionResult> Crear()
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            await CargarSelectListsAsync();
            return View(new Nota());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Nota nota)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            ModelState.Remove("Estudiante");
            ModelState.Remove("Materia");

            if (!ModelState.IsValid) { await CargarSelectListsAsync(); return View(nota); }

            nota.FechaRegistro = DateTime.Now;
            _context.Notas.Add(nota);
            await _context.SaveChangesAsync();

            _context.MovimientosNotas.Add(new MovimientoNota
            {
                NotaId = nota.Id,
                EstudianteId = nota.EstudianteId,
                MateriaId = nota.MateriaId,
                TipoMovimiento = "Insercion",
                ValorNuevo = nota.Valor,
                Periodo = nota.Periodo,
                UsuarioAccion = UsuarioActual(),
                FechaMovimiento = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Nota creada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Notas/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var nota = await _context.Notas.Include(n => n.Estudiante).Include(n => n.Materia).FirstOrDefaultAsync(n => n.Id == id);
            if (nota == null) return NotFound();
            await CargarSelectListsAsync(null, nota.EstudianteId, nota.MateriaId);
            return View(nota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Nota nota)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            if (id != nota.Id) return BadRequest();
            ModelState.Remove("Estudiante");
            ModelState.Remove("Materia");

            if (!ModelState.IsValid) { await CargarSelectListsAsync(null, nota.EstudianteId, nota.MateriaId); return View(nota); }

            var original = await _context.Notas.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id);
            if (original == null) return NotFound();

            try
            {
                _context.Update(nota);
                await _context.SaveChangesAsync();

                _context.MovimientosNotas.Add(new MovimientoNota
                {
                    NotaId = nota.Id,
                    EstudianteId = nota.EstudianteId,
                    MateriaId = nota.MateriaId,
                    TipoMovimiento = "Modificacion",
                    ValorAnterior = original.Valor,
                    ValorNuevo = nota.Valor,
                    Periodo = nota.Periodo,
                    UsuarioAccion = UsuarioActual(),
                    FechaMovimiento = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Nota actualizada.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Notas.AnyAsync(n => n.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // ── ELIMINAR
        // GET: Notas/EliminarNota
        public async Task<IActionResult> EliminarNota(int? estudianteId, int? materiaId, string? periodo)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");

            Nota? nota = null;
            if (estudianteId.HasValue && materiaId.HasValue && !string.IsNullOrEmpty(periodo))
            {
                nota = await _context.Notas
                    .Include(n => n.Estudiante)
                    .Include(n => n.Materia)
                    .FirstOrDefaultAsync(n => n.EstudianteId == estudianteId && n.MateriaId == materiaId && n.Periodo == periodo);
            }

            ViewBag.Nota = nota;
            ViewBag.Estudiantes = new SelectList(await _context.Estudiantes.OrderBy(e => e.Apellido).ToListAsync(), "Id", "NombreCompleto", estudianteId);
            ViewBag.Materias = new SelectList(await _context.Materias.OrderBy(m => m.Nombre).ToListAsync(), "Id", "Nombre", materiaId);
            ViewBag.Periodos = await _context.Notas.Select(n => n.Periodo).Distinct().OrderByDescending(p => p).ToListAsync();
            ViewBag.Periodo = periodo;
            return View();
        }

        // POST: Notas/EliminarNota
        [HttpPost, ActionName("EliminarNota")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarNotaConfirmado(int estudianteId, int materiaId, string periodo)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");

            var nota = await _context.Notas.FirstOrDefaultAsync(n =>
                n.EstudianteId == estudianteId && n.MateriaId == materiaId && n.Periodo == periodo);

            if (nota == null)
            {
                TempData["Error"] = "No se encontró la nota especificada.";
                return RedirectToAction("EliminarNota");
            }

            _context.MovimientosNotas.Add(new MovimientoNota
            {
                NotaId = nota.Id,
                EstudianteId = nota.EstudianteId,
                MateriaId = nota.MateriaId,
                TipoMovimiento = "Eliminacion",
                ValorAnterior = nota.Valor,
                Periodo = nota.Periodo,
                UsuarioAccion = UsuarioActual(),
                FechaMovimiento = DateTime.Now
            });

            _context.Notas.Remove(nota);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Nota eliminada y movimiento registrado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Notas/Eliminar/5 (CRUD estándar)
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var nota = await _context.Notas.Include(n => n.Estudiante).Include(n => n.Materia).FirstOrDefaultAsync(n => n.Id == id);
            if (nota == null) return NotFound();
            return View(nota);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            if (!EsAdminOProfesor()) return RedirectToAction("Index", "Home");
            var nota = await _context.Notas.FindAsync(id);
            if (nota != null)
            {
                _context.MovimientosNotas.Add(new MovimientoNota
                {
                    NotaId = nota.Id,
                    EstudianteId = nota.EstudianteId,
                    MateriaId = nota.MateriaId,
                    TipoMovimiento = "Eliminacion",
                    ValorAnterior = nota.Valor,
                    Periodo = nota.Periodo,
                    UsuarioAccion = UsuarioActual(),
                    FechaMovimiento = DateTime.Now
                });
                _context.Notas.Remove(nota);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Nota eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }

        //  MOVIMIENTO_NOTAS (solo lectura/consulta)
        public async Task<IActionResult> MovimientoNotas(string? tipo, int? estudianteId, int? materiaId, DateTime? desde, DateTime? hasta)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            var query = _context.MovimientosNotas
                .Include(m => m.Estudiante)
                .Include(m => m.Materia)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipo)) query = query.Where(m => m.TipoMovimiento == tipo);
            if (estudianteId.HasValue) query = query.Where(m => m.EstudianteId == estudianteId);
            if (materiaId.HasValue) query = query.Where(m => m.MateriaId == materiaId);
            if (desde.HasValue) query = query.Where(m => m.FechaMovimiento >= desde.Value);
            if (hasta.HasValue) query = query.Where(m => m.FechaMovimiento <= hasta.Value.AddDays(1));

            ViewBag.Tipo = tipo;
            ViewBag.EstudianteId = estudianteId;
            ViewBag.MateriaId = materiaId;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Estudiantes = new SelectList(await _context.Estudiantes.OrderBy(e => e.Apellido).ToListAsync(), "Id", "NombreCompleto", estudianteId);
            ViewBag.Materias = new SelectList(await _context.Materias.OrderBy(m => m.Nombre).ToListAsync(), "Id", "Nombre", materiaId);

            return View(await query.OrderByDescending(m => m.FechaMovimiento).ToListAsync());
        }

        //  INFORMES POR PARÁMETROS
        public async Task<IActionResult> Informes(string? estudianteId, int? materiaId, string? periodo, decimal? notaMin, decimal? notaMax)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            var rol = HttpContext.Session.GetString("UsuarioRol");
            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");

            var query = _context.Notas
                .Include(n => n.Estudiante)
                .Include(n => n.Materia)
                    .ThenInclude(m => m!.Docente)
                .AsQueryable();

            if (rol == "Estudiante")
            {
                var est = await _context.Estudiantes.Include(e => e.Usuario)
                    .FirstOrDefaultAsync(e => e.Usuario != null && e.Usuario.NombreUsuario == usuarioNombre);
                if (est != null) query = query.Where(n => n.EstudianteId == est.Id);
                else query = query.Where(n => false);
            }

            List<Nota> notas = new();

            if (!string.IsNullOrEmpty(estudianteId))
            {
                if (int.TryParse(estudianteId, out int estId))
                    query = query.Where(n => n.EstudianteId == estId);
                else
                    query = query.Where(n => (n.Estudiante!.Nombre + " " + n.Estudiante.Apellido).Contains(estudianteId));
            }
            if (materiaId.HasValue) query = query.Where(n => n.MateriaId == materiaId);
            if (!string.IsNullOrEmpty(periodo)) query = query.Where(n => n.Periodo == periodo);
            if (notaMin.HasValue) query = query.Where(n => n.Valor >= notaMin.Value);
            if (notaMax.HasValue) query = query.Where(n => n.Valor <= notaMax.Value);

            // Solo consulta si hay al menos un filtro aplicado
            if (!string.IsNullOrEmpty(estudianteId) || materiaId.HasValue || !string.IsNullOrEmpty(periodo) || notaMin.HasValue || notaMax.HasValue)
                notas = await query.OrderBy(n => n.Estudiante!.Apellido).ThenBy(n => n.Materia!.Nombre).ToListAsync();

            var vm = new ReporteNotasViewModel
            {
                EstudianteNombre = estudianteId,
                Periodo = periodo,
                NotaMin = notaMin,
                NotaMax = notaMax,
                Notas = notas,
                EstudiantesLista = await _context.Estudiantes.OrderBy(e => e.Apellido).ToListAsync(),
                MateriasLista = await _context.Materias.OrderBy(m => m.Nombre).ToListAsync()
            };
            ViewBag.MateriaId = materiaId;
            return View(vm);
        }

        //  GRÁFICOS POR PARÁMETROS
        public async Task<IActionResult> Graficos(int? materiaId, string? periodo, string? agrupacion)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Account");

            agrupacion ??= "materia";

            var query = _context.Notas
                .Include(n => n.Materia)
                .Include(n => n.Estudiante)
                .AsQueryable();

            if (materiaId.HasValue) query = query.Where(n => n.MateriaId == materiaId);
            if (!string.IsNullOrEmpty(periodo)) query = query.Where(n => n.Periodo == periodo);

            var notas = await query.ToListAsync();

            List<GraficoDataPoint> datos = agrupacion switch
            {
                "estudiante" => notas.GroupBy(n => n.Estudiante!.NombreCompleto)
                    .Select(g => new GraficoDataPoint { Etiqueta = g.Key, Promedio = Math.Round(g.Average(n => n.Valor), 2), TotalEstudiantes = g.Count() })
                    .OrderBy(d => d.Etiqueta).ToList(),
                "periodo" => notas.GroupBy(n => n.Periodo)
                    .Select(g => new GraficoDataPoint { Etiqueta = g.Key, Promedio = Math.Round(g.Average(n => n.Valor), 2), TotalEstudiantes = g.Select(n => n.EstudianteId).Distinct().Count() })
                    .OrderBy(d => d.Etiqueta).ToList(),
                _ => notas.GroupBy(n => n.Materia!.Nombre)
                    .Select(g => new GraficoDataPoint { Etiqueta = g.Key, Promedio = Math.Round(g.Average(n => n.Valor), 2), TotalEstudiantes = g.Select(n => n.EstudianteId).Distinct().Count() })
                    .OrderByDescending(d => d.Promedio).ToList()
            };

            var vm = new GraficoViewModel
            {
                MateriaSeleccionada = materiaId?.ToString(),
                Periodo = periodo,
                MateriasLista = await _context.Materias.OrderBy(m => m.Nombre).ToListAsync(),
                Datos = datos
            };

            ViewBag.Agrupacion = agrupacion;
            ViewBag.MateriaId = materiaId;
            ViewBag.Periodos = await _context.Notas.Select(n => n.Periodo).Distinct().OrderByDescending(p => p).ToListAsync();
            return View(vm);
        }

        // ── Helpers ────────────────────────────────────────────────
        private async Task CargarSelectListsAsync(RegistroNotaViewModel? vm = null, int? estId = null, int? matId = null)
        {
            var estudiantes = await _context.Estudiantes
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();

            var materias = await _context.Materias
                .Include(m => m.Docente)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            // Para las vistas Crear/Editar (usan ViewBag con SelectList)
            ViewBag.EstudianteId = new SelectList(
                estudiantes, "Id", "NombreCompleto", vm?.EstudianteId ?? estId);
            ViewBag.MateriaId = new SelectList(
                materias, "Id", "Nombre", vm?.MateriaId ?? matId);

            // Para la vista Registrar (usa Model.Estudiantes y Model.Materias directamente)
            if (vm != null)
            {
                vm.Estudiantes = estudiantes;
                vm.Materias = materias;
            }
        }
    }
}
