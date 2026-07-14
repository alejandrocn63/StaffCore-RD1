using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffCore_RD1.Data;
using StaffCore_RD1.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StaffCoreRD.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly StaffDbContext _context;

        public StaffController(StaffDbContext context)
        {
            _context = context;
        }

        // GET: Staff (Index) -- AHORA CON BUSCADOR Y ESTADÍSTICAS
        [Authorize(Roles = "Administrador,RRHH,Viewer")]
        public async Task<IActionResult> Index(string buscarNombre)
        {
            // BONUS 3: Resumen estadístico usando LINQ GroupBy
            var estadisticas = await _context.Personal
                .Where(s => s.Activo)
                .GroupBy(s => s.Departamento)
                .Select(g => new
                {
                    Departamento = g.Key,
                    TotalEmpleados = g.Count(),
                    TotalNomina = g.Sum(s => s.Salario)
                }).ToListAsync();

            ViewBag.Estadisticas = estadisticas; // Lo enviamos a la vista

            // BONUS 1: Buscador con LINQ .Contains()
            var consulta = _context.Personal.Where(s => s.Activo);

            if (!string.IsNullOrEmpty(buscarNombre))
            {
                consulta = consulta.Where(s => s.Nombre.Contains(buscarNombre));
            }

            var personalActivo = await consulta.OrderBy(s => s.Nombre).ToListAsync();
            return View(personalActivo);
        }

        // BONUS 2: GET: Staff/Details/5 (Perfil Completo)
        [Authorize(Roles = "Administrador,RRHH,Viewer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Personal.FindAsync(id);
            if (staff == null) return NotFound();

            return View(staff);
        }

   

        // GET: Staff/Create
        [Authorize(Roles = "Administrador,RRHH")]
        public IActionResult Create()
        {
            return View(new Staff());
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,RRHH")]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (ModelState.IsValid)
            {
                _context.Add(staff);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Empleado registrado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Staff/Edit/5
        [Authorize(Roles = "Administrador,RRHH")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Personal.FindAsync(id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // POST: Staff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,RRHH")]
        public async Task<IActionResult> Edit(int id, Staff staff)
        {
            if (id != staff.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Datos del empleado actualizados.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Personal.Any(e => e.Id == staff.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Staff/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Personal.FindAsync(id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // POST: Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Personal.FindAsync(id);
            if (staff != null)
            {
                _context.Personal.Remove(staff);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Empleado eliminado del sistema.";
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Staff/Privacy
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}