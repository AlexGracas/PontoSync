﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PontoSync.Data;
using PontoSync.Service.Cron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PontoSync.Service
{
    /// <summary>
    /// Inspirado nó código: https://codeburst.io/schedule-cron-jobs-using-hostedservice-in-asp-net-core-e17c47ba06
    /// </summary>
    public class LeituraRelogioCronJob : CronJobService
    {
        private readonly ILogger<LeituraRelogioCronJob> _logger;

        private readonly IServiceProvider _serviceProvider;


        public LeituraRelogioCronJob(IScheduleConfig<LeituraRelogioCronJob> config, ILogger<LeituraRelogioCronJob> logger,
            IServiceProvider serviceProvider
            )
       : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de Leitura Relógio Iniciado.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Iniciando ciclo e leitura.");
                using var scope = _serviceProvider.CreateScope();
                var _pontoSyncContext = scope.ServiceProvider.GetRequiredService<PontoSyncContext>();
                var _frequenciaContext = scope.ServiceProvider.GetRequiredService<FrequenciaContext>();
                var relogios = _pontoSyncContext.Relogios.ToList();
                int count = 0;
                Parallel.ForEach(relogios, relogio =>
                {
                    try
                    {
                        IRelogioService relogioService = (IRelogioService)ActivatorUtilities.CreateInstance(_serviceProvider.CreateScope().ServiceProvider, typeof(RelogioHenry));
                        DateTime inicio;
                        //Se nunca havia sido lido antes, ler os últimos 60 minutos.
                        if (relogio.UltimoSucesso == null)
                        {
                            inicio = DateTime.Now.AddMinutes(-60);
                        }
                        else
                        {
                            //Se já havia sido lido, recuperar no máximo 3 dias de leitura.
                            if((DateTime.Now.Date - relogio.UltimoSucesso.Value.Date).TotalDays > 3)
                            {
                                inicio = DateTime.Now.AddDays(-3);
                            }
                            //Se a leitura é recente ler desde o último sucesso, e 15 minutos de sobreposição.
                            else
                            {
                                inicio = relogio.UltimoSucesso.Value.AddMinutes(-15);
                            }
                            
                        }                                                
                        relogioService.LerRelogioELancarAsync(relogio, inicio , DateTime.Now, true);
                    }
                    catch (Exception e)
                    {
                        count++;
                        _logger.LogError(1, e, $"Problema lendo o relógio {relogio.Nome} com o IP {relogio.URL} e descrito como {relogio.Descricao}");
                    }
                }
                );

                _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Terminado ciclo e leitura. Quantidade de erros:" + count);

            }
            catch{
                _logger.LogError("Erro crítico no Job de leitura do relógio");
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de Leitura Relógio Terminado.");
            return base.StopAsync(cancellationToken);
        }
    }
}
