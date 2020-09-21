using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PontoSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoSync.Service
{
    public class MarcacaoFrequenciaService
    {
        private readonly Data.FrequenciaContext _frequenciaContext;
        private readonly Data.PontoSyncContext _pontoSyncContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MarcacaoFrequenciaService> _logger;
        public MarcacaoFrequenciaService(Data.FrequenciaContext frequenciaContext, Data.PontoSyncContext pontoSyncContext,
                        IServiceProvider serviceProvider, ILogger<MarcacaoFrequenciaService> logger)
        {
            _frequenciaContext = frequenciaContext;
            _pontoSyncContext = pontoSyncContext;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public bool VerificarDuplicado(Registro registro, string Matricula)
        {
            if (_frequenciaContext.MarcacaoFrequencia.Any(
            mf => mf.DataMarcacao == registro.Marcacao.Date &&
            mf.MatServidor == Matricula &&
            (mf.HoraMarcacao == registro.Marcacao.AddMinutes(1).ToString("HH:mm") ||
            mf.HoraMarcacao == registro.Marcacao.ToString("HH:mm") ||
            mf.HoraMarcacao == registro.Marcacao.AddMinutes(-1).ToString("HH:mm"))
            )
            )
            {
                return true;
            }
            return false;
        }

        public bool VerificarMigrado(Registro registro)
        {
            try
            {
                var servidor = _frequenciaContext.Servidores.FromSqlRaw($"select s.mat_servidor, s.nom, s.e_mail from srh2.servidor s where s.mat_servidor = {int.Parse(registro.Matricula)} ").FirstOrDefault();
                if(servidor == null)
                {
                    throw new Exception($"Erro, não existe o servidor {registro.Matricula} no Banco SGRH.");
                }
                if (VerificarDuplicado(registro, servidor.Matricula))
                {
                    _logger.LogWarning("Tentando lançar registro duplicado.");
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro ao Verificar se o registro do servidor {registro.Matricula} na hora {registro.Marcacao} já foi migrado.");
            }
            return false;
        }

        public void LancarRegistro(Registro registro)
        {
            int lastUsedId = _frequenciaContext.MarcacaoFrequencia.Max(mf => mf.Id);
            var r = _pontoSyncContext.Registros.Find(registro.Id);
            try
            {
                var servidor = _frequenciaContext.Servidores.FromSqlRaw($"select s.mat_servidor, s.nom, s.e_mail from srh2.servidor s where s.mat_servidor = {int.Parse(registro.Matricula)} ").FirstOrDefault();
                MarcacaoFrequencia marcacao = new MarcacaoFrequencia(++lastUsedId, registro, servidor.Matricula);
                if (VerificarDuplicado(registro, servidor.Matricula))
                {
                    _logger.LogWarning("Tentando lançar registro duplicado.");
                    r.Migrado = true;
                    return;
                }
                _frequenciaContext.MarcacaoFrequencia.Add(marcacao);
                _frequenciaContext.SaveChanges();               
                r.Migrado = true;              
            }catch (Exception e){
                r.Migrado = false;
                _pontoSyncContext.SaveChanges();
                _logger.LogError(e, $"Erro ao lançar o registro do servidor {registro.Matricula} na hora {registro.Marcacao}");
            }
            finally
            {
                _pontoSyncContext.Update(r);
                _pontoSyncContext.SaveChanges();
            }
}

        public void LancarRegistros(ICollection<Registro> registros)
        {
            foreach (var registro in registros.Where(r => !r.Migrado && r.Id != 0)){                
                    LancarRegistro(registro);                
             }
        }
    }
}
