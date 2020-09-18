using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PontoSync.Data;
using PontoSync.Models;

namespace PontoSync.Controllers
{
    public class RegistrosController : Controller
    {
        private readonly PontoSyncContext _context;

        public RegistrosController(PontoSyncContext context)
        {
            _context = context;
        }

        // GET: Registros
        [HttpGet("{controller}/Index/{matricula?}/{Migrado?}")]
        public async Task<IActionResult> Index(String matricula = "", String Migrado = "")
        {

            var pontoSyncContext = _context.Registros.Include(r => r.Relogio);
            if (!String.IsNullOrEmpty(matricula))
            {
                ViewBag.Matricula = matricula.ToString();
                pontoSyncContext.Where(ps => ps.Matricula.EndsWith(matricula.ToString()));
            }
            else
            {
                ViewBag.Matricula = "";
            }
            if(Migrado == "NMigrado")
            {
                pontoSyncContext.Where(ps => ps.MigradoFrequencia == "F");
            }else if(Migrado == "Migrado")
            {
                pontoSyncContext.Where(ps => ps.MigradoFrequencia == "T");
            }
            return View(await pontoSyncContext.ToListAsync());
        }


        // GET: Registros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registro = await _context.Registros
                .Include(r => r.Relogio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }

        // GET: Registros/Create
        public IActionResult Create()
        {
            ViewData["IdRelogio"] = new SelectList(_context.Relogios, "Id", "Id");
            return View();
        }

        // POST: Registros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,idMarcacaoRelogio,Matricula,Marcacao,IdRelogio,MigradoFrequencia")] Registro registro)
        {
            if (ModelState.IsValid)
            {
                _context.Add(registro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRelogio"] = new SelectList(_context.Relogios, "Id", "Id", registro.IdRelogio);
            return View(registro);
        }

        // GET: Registros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registro = await _context.Registros.FindAsync(id);
            if (registro == null)
            {
                return NotFound();
            }
            ViewData["IdRelogio"] = new SelectList(_context.Relogios, "Id", "Id", registro.IdRelogio);
            return View(registro);
        }

        // POST: Registros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,idMarcacaoRelogio,Matricula,Marcacao,IdRelogio,MigradoFrequencia")] Registro registro)
        {
            if (id != registro.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistroExists(registro.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRelogio"] = new SelectList(_context.Relogios, "Id", "Id", registro.IdRelogio);
            return View(registro);
        }

        // GET: Registros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registro = await _context.Registros
                .Include(r => r.Relogio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }

        // POST: Registros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registro = await _context.Registros.FindAsync(id);
            _context.Registros.Remove(registro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegistroExists(int id)
        {
            return _context.Registros.Any(e => e.Id == id);
        }
    }
}
