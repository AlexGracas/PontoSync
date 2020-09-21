using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PontoSync.Data;
using PontoSync.Models;

namespace PontoSync.Controllers
{
    [Authorize]
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

            var pontoSyncContext = _context.Registros.Include(r => r.Relogio).Where(p=> true);
            if (!String.IsNullOrEmpty(matricula))
            {
                ViewBag.Matricula = matricula.ToString();
                pontoSyncContext = pontoSyncContext.Where(ps => ps.Matricula.EndsWith(matricula.ToString()));
            }
            else
            {
                ViewBag.Matricula = "";
            }
            if(Migrado == "NMigrado")
            {
                pontoSyncContext = pontoSyncContext.Where(ps => ps.MigradoFrequencia == "F");
            }else if(Migrado == "Migrado")
            {
                pontoSyncContext = pontoSyncContext.Where(ps => ps.MigradoFrequencia == "T");
            }
            return View(await pontoSyncContext.OrderByDescending(ps=> ps.Marcacao).ToListAsync());
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

     
       
        private bool RegistroExists(int id)
        {
            return _context.Registros.Any(e => e.Id == id);
        }
    }
}
