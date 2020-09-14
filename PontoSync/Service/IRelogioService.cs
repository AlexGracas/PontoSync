using PontoSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoSync.Service
{
    public interface IRelogioService
    {

        public Task<ICollection<Registro>> ObterRegistrosAsync(Relogio relogio, DateTime Inicio, DateTime Fim);
        public ICollection<Registro> ObterRegistros(Relogio relogio, DateTime Inicio, DateTime Fim);

        public Task<ICollection<Registro>> ObterRegistrosAsync(Relogio relogio);
        public ICollection<Registro> ObterRegistros(Relogio relogio);

        public Task LerRelogioELancarAsync(Relogio relogio, DateTime Inicio, DateTime Fim, Boolean lancar = false);
        public void LancarRegistro(Relogio relogio, Registro registro);

        public void LancarRegistros(Relogio relogio, ICollection<Registro> registro);

    }
}
