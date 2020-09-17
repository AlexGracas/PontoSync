using NUnit.Framework;
using PontoSync.Models;
using PontoSync.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestePontoSync
{
    class PontoSyncUnitTest
    {
        [Test]
        public void LeituraTest() {
            Relogio relogio = new Relogio();
            relogio.Nome = "ABC";
            relogio.URL = "http://10.6.9.7";
            relogio.Usuario = "primmesf";
            relogio.Senha = "121314";
            IRelogioService relogioService = new RelogioHenry(null,null,null,null);
            var registros = relogioService.ObterRegistros(relogio, DateTime.Now.AddDays(-1), DateTime.Now);
            foreach(var r in registros)
            {
                Console.WriteLine(r.Marcacao);
            }
            
        }
    }


}
