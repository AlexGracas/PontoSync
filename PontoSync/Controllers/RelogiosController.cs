using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PontoSync.Data;
using PontoSync.Models;
using PontoSync.Service;

namespace PontoSync.Controllers
{
    public class RelogiosController : Controller
    {
        private readonly PontoSyncContext _context;
        private readonly FrequenciaContext _frequenciaContext;
        private readonly ILogger<RelogiosController> _logger;
        private readonly IServiceProvider _serviceProvider;
        public RelogiosController(PontoSyncContext context, FrequenciaContext frequenciaContext, ILogger<RelogiosController> logger,
            IServiceProvider provider)
        {
            _context = context;
            _frequenciaContext = frequenciaContext;
            _logger = logger;
            _serviceProvider = provider;
        }

        // GET: Relogios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Relogios.ToListAsync());
        }

        // GET: Relogios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            return await Details(id, DateTime.Now.AddDays(-7), DateTime.Now);
        }

        [HttpGet("{controller}/MigrarIntervalo/{id}/{Inicio}/{Fim}")]
        public async Task<IActionResult> MigrarIntervalo(int? id, DateTime inicio, DateTime final)
        {
            if (id == null)
            {
                return NotFound();
            }           
            var relogio = await _context.Relogios.FindAsync(id);
            var registro = await _context.Registros.Where(r => r.Marcacao >= inicio && r.Marcacao <= final 
                                && r.IdRelogio == id && r.MigradoFrequencia == "F").ToListAsync();
            if (registro == null)
            {
                return NotFound();
            }
            IRelogioService relogioService = (IRelogioService)ActivatorUtilities.CreateInstance(this._serviceProvider, typeof(RelogioHenry));
            relogioService.LancarRegistros(relogio, registro);
            relogio.Registros = registro;
            ViewBag.Fim = final;
            ViewBag.Inicio = inicio;
            return View("Details", relogio);
        }


        public async Task<IActionResult> Migrar(int? id, int? idRegistro)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (idRegistro == null)
            {
                return NotFound();
            }
            try
            {
                var relogio = await _context.Relogios.FindAsync(id);
                var registro = await _context.Registros.FindAsync(idRegistro);
                if (registro == null)
                {
                    return NotFound();
                }
                IRelogioService relogioService = (IRelogioService)ActivatorUtilities.CreateInstance(this._serviceProvider, typeof(RelogioHenry));
                relogioService.LancarRegistro(relogio, registro);
                ViewBag.Fim = registro.Marcacao.Date;
                ViewBag.Inicio = registro.Marcacao.Date;
                return View("Details", relogio);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Erro ao migrar Relógio {id} e o registro {idRegistro}");
                throw e;
            }

        }

        // GET: Relogios/Details/5
        [HttpGet("{controller}/Details/{id}/{Inicio?}/{Fim?}")]
        public async Task<IActionResult> Details(int? id, DateTime? Inicio, DateTime? Fim )
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!Inicio.HasValue)
            {
                Inicio = DateTime.Now.AddDays(-3).Date;
            }
            if (!Fim.HasValue)
            {
                Fim = DateTime.Now.Date;
            }
            ViewBag.Fim =  Fim;
            ViewBag.Inicio = Inicio;
            var relogio = await _context.Relogios
                .FirstOrDefaultAsync(m => m.Id == id);
            IRelogioService relogioService = (IRelogioService)ActivatorUtilities.CreateInstance(this._serviceProvider, typeof(RelogioHenry));
            await relogioService.LerRelogioELancarAsync(relogio, Inicio.Value, Fim.Value, lancar: false);

            relogio.Registros = _context.Registros.Where(r => r.IdRelogio == relogio.Id && r.Marcacao >Inicio && r.Marcacao < Fim).OrderBy(r=> r.Marcacao).ToList();

            if (relogio == null)
            {
                return NotFound();
            }

            return View(relogio);
        }

        // GET: Relogios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Relogios/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,URL,Nome,Descricao,UltimaLeitura,UltimoSucess,UltimaFalha,Usuario,Senha")] Relogio relogio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(relogio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(relogio);
        }

        // GET: Relogios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var relogio = await _context.Relogios.Where(R => R.Id == id).FirstOrDefaultAsync();
            if (relogio == null)
            {
                return NotFound();
            }
            return View(relogio);
        }




        // POST: Relogios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,URL,Descricao,Nome,Usuario, Senha")] Relogio relogio)
        {
            if (id != relogio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(relogio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RelogioExists(relogio.Id))
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
            return View(relogio);
        }

        // GET: Relogios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var relogio = await _context.Relogios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relogio == null)
            {
                return NotFound();
            }

            return View(relogio);
        }

        // POST: Relogios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var relogio = await _context.Relogios.FindAsync(id);
            _context.Relogios.Remove(relogio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RelogioExists(int id)
        {
            return _context.Relogios.Any(e => e.Id == id);
        }
    }
}
