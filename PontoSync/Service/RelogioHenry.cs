using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PontoSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PontoSync.Service
{
    public class RelogioHenry: IRelogioService
    {

        String Download = @"rep.html?pgCode=8&opType=5&lblId=0&visibleDiv=info&lblNsrI=000000001&lblNsrF=000142664&lblDataI=27/01/20+15:57&lblDataF=13/03/20+18:00&fileinput=";

        private readonly ILogger<RelogioHenry> _logger;
        private readonly Data.PontoSyncContext _context;
        private readonly Data.FrequenciaContext _frequenciaContext;
        private readonly IServiceProvider _provider;
        public RelogioHenry(Data.PontoSyncContext context, Data.FrequenciaContext frequenciaContext, ILogger<RelogioHenry> logger,
            IServiceProvider provider
            )
        {
            this._context = context;
            this._frequenciaContext = frequenciaContext;
            this._logger = logger;
            _provider = provider;
        }

        private String LoginURL(String Usuario, String Senha)
        {
            return @"rep.html?pgCode=7&opType=1&lblId=0&lblLogin=" + Usuario + "&lblPass=" + Senha;
        }

        private String DownloadURL(DateTime Inicio, DateTime Fim)
        {
            string param = "";
            if (Inicio != DateTime.MinValue || Fim != DateTime.MaxValue)
            {
                param = "&lblDataI=" +
                    Inicio.ToString("dd/MM/yy+HH:mm") +
                    //27/01/20+15:57
                    @"&lblDataF=" +
                    Fim.ToString("dd/MM/yy+HH:mm") +
                    //13/03/20+18:00
                    @"&fileinput=";
                param = param.Replace("/", "%2F");
                param = param.Replace(":", "%3A");
            }
            return DownloadURL() + param;
        }

        private String DownloadURL()
        {
            return @"rep.html?pgCode=8&opType=5&lblId=2&visibleDiv=info";
        }

        private async Task<bool> Login(Relogio relogio)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(relogio.URL.ToString() + LoginURL(relogio.Usuario, relogio.Senha));
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Loggin Realizado com sucesso no relógio: " + relogio.Nome);
                return true;
            }catch(Exception e)
            {
                _logger.LogError(e,"Erro ao realizar login no relógio: " + relogio.Nome);
                return false; ;
            }
        }


        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<ICollection<Registro>> ObterRegistrosAsync(Relogio relogio, DateTime Inicio, DateTime Fim)
        {
            try
            {
                relogio = _context.Relogios.Find(relogio.Id);
                if (!(await Login(relogio)))
                {
                    throw new Exception("Não foi possível realizar login no relógio");
                }
                
                relogio.UltimaLeitura = DateTime.Now;
                String urlNow = relogio.URL.ToString() + DownloadURL(Inicio, Fim);
                HttpResponseMessage response = await HttpClient.GetAsync(urlNow);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var registros = CriarRegistrosDeTexto(relogio, responseBody);

                _logger.LogInformation($"Lido {registros.Count} do relógio {relogio.Nome}");
                VerificarRegistrosEAtualizarBanco(registros);
                relogio.UltimoSucesso = relogio.UltimaLeitura;
                _logger.LogInformation($"Marcações do relógio {relogio.Nome} lançadas com sucesso.");
                return registros;
            }
            catch(Exception e)
            {
                relogio.UltimaFalha = relogio.UltimaLeitura;
                _logger.LogError(e, $"Erro ao realizar leitura do relógio {relogio.Nome}.");
                throw e;
            }
            finally
            {
                _context.SaveChanges();
            }
        }

        public async Task<ICollection<Registro>> ObterRegistrosAsync(Relogio relogio)
        {
            return await ObterRegistrosAsync(relogio, DateTime.MinValue, DateTime.MaxValue);
        }

        public ICollection<Registro> ObterRegistros(Relogio relogio, DateTime Inicio, DateTime Fim)
        {
            return this.ObterRegistrosAsync( relogio,  Inicio,  Fim).Result;
        }

        ICollection<Registro> IRelogioService.ObterRegistros(Relogio relogio)
        {
            return this.ObterRegistrosAsync(relogio).Result;
        }

        private static ICollection<Registro> CriarRegistrosDeTexto(Relogio relogio, String texto)
        {
            List<Registro> registros = new List<Registro>();
            foreach (var linha in texto.Replace("\r", "").Split("\n"))
            {
                if (linha.Length > 1)
                {
                    Registro novo = new Registro();
                    var termos = linha.Split("[");
                    novo.idMarcacaoRelogio = termos[0];
                    novo.Matricula = termos[2];
                    novo.Marcacao = DateTime.Parse(termos[3]);
                    novo.Relogio = relogio;
                    novo.IdRelogio = relogio.Id;
                    registros.Add(novo);
                }
            }
            return registros;
        }

        private void VerificarRegistrosEAtualizarBanco(ICollection<Registro> registros)
        {
            //Cria o serviço, usando Injeção de Dependência.
            MarcacaoFrequenciaService marcacaoFrequenciaService =
                (MarcacaoFrequenciaService)ActivatorUtilities.CreateInstance(_provider, typeof(MarcacaoFrequenciaService));
            foreach (var registro in registros)
            {
                var registroAdicionavel = !_context.Registros.Where(r => r.Matricula.CompareTo(registro.Matricula)==0
                                        && r.IdRelogio == registro.IdRelogio
                                        && r.Marcacao == registro.Marcacao).Any();
                if (registroAdicionavel)
                {
                    _context.Registros.Add(new Registro() { 
                    IdRelogio = registro.IdRelogio,
                    Marcacao = registro.Marcacao,
                    Matricula = registro.Matricula,
                    Migrado = marcacaoFrequenciaService.VerificarMigrado(registro),
                    idMarcacaoRelogio = registro.idMarcacaoRelogio});
                }
            }
            _context.SaveChanges();
        }

        public async Task LerRelogioELancarAsync(Relogio relogio, DateTime Inicio, DateTime Fim, bool lancar = false)
        {
            //Função obtem registros e salva no banco.
            await this.ObterRegistrosAsync(relogio, Inicio, Fim);
            //Recupera registro do banco.
            var registrosParaEnvio = _context.Registros.Include(r=>r.Relogio).AsNoTracking().Where(
                reg => reg.MigradoFrequencia == "F" && 
                reg.IdRelogio == relogio.Id && 
                reg.Marcacao >= Inicio && 
                reg.Marcacao <= Fim)
                .Select(reg => reg).ToList();
            if (lancar)
            {
                //Cria o serviço, usando Injeção de Dependência.
                MarcacaoFrequenciaService marcacaoFrequenciaService =
                    (MarcacaoFrequenciaService)ActivatorUtilities.CreateInstance(_provider, typeof(MarcacaoFrequenciaService));
                //Lança registro no SRH2
                marcacaoFrequenciaService.LancarRegistros(registrosParaEnvio);
            }
        }


        public void LancarRegistro(Relogio relogio, Registro registro)
        {           
            //Cria o serviço, usando Injeção de Dependência.
            MarcacaoFrequenciaService marcacaoFrequenciaService =
                (MarcacaoFrequenciaService)ActivatorUtilities.CreateInstance(_provider, typeof(MarcacaoFrequenciaService));
            //Lança registro no SRH2
            marcacaoFrequenciaService.LancarRegistro(registro);
        }

        public void LancarRegistros(Relogio relogio, ICollection<Registro> registros)
        {
            //Cria o serviço, usando Injeção de Dependência.
            MarcacaoFrequenciaService marcacaoFrequenciaService =
                (MarcacaoFrequenciaService)ActivatorUtilities.CreateInstance(_provider, typeof(MarcacaoFrequenciaService));
            //Lança registro no SRH2
            marcacaoFrequenciaService.LancarRegistros(registros);
        }

    }

    public class RelogioHenryResultWorker
    {
        public List<String> Linhas;

        public List<Registro> GerarMarcacoes(String response)
        {
            List<Registro> registros = new List<Registro>();
            foreach(String linha in Linhas)
            {
                registros.Add(new Registro()
                {
                    idMarcacaoRelogio = linha.Substring(0,10),
                    Matricula = linha.Substring(15, 35),
                    Marcacao = new DateTime(
                    year: int.Parse(linha.Substring(42, 46)),
                    month: int.Parse(linha.Substring(39, 41)),
                    day: int.Parse(linha.Substring(36, 38)),
                    hour: int.Parse(linha.Substring(47,49)),
                    minute: int.Parse(linha.Substring(50,52)),
                    second: int.Parse(linha.Substring(53,55))
                    )
                });
            }
            return registros;
        }
    }
}


