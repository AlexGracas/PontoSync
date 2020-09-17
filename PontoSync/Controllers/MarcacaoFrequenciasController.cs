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
    public class MarcacaoFrequenciasController : Controller
    {
        private readonly FrequenciaContext _context;

        public MarcacaoFrequenciasController(FrequenciaContext context)
        {
            _context = context;
        }

        // GET: MarcacaoFrequencias
        public async Task<IActionResult> Index()
        {
            return View(await _context.MarcacaoFrequencia.Where(mf => mf.DataMarcacao > DateTime.Now.AddDays(-7)).OrderBy(mf=> mf.DataMarcacao ).ToListAsync());
        }

        // GET: MarcacaoFrequencias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marcacaoFrequencia = await _context.MarcacaoFrequencia
                .FirstOrDefaultAsync(m => m.Id == id.Value);
            if (marcacaoFrequencia == null)
            {
                return NotFound();
            }

            return View(marcacaoFrequencia);
        }

        private bool MarcacaoFrequenciaExists(int id)
        {
            return _context.MarcacaoFrequencia.Any(e => e.Id == id);
        }

    }
}
