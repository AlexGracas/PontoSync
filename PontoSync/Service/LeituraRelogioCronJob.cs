using Microsoft.Extensions.DependencyInjection;
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
                    //IRelogioService relogioService = new RelogioHenry(_pontoSyncContext, _frequenciaContext,_logger);
                    relogioService.LerRelogioELancarAsync(relogio, DateTime.Now.AddHours(-12), DateTime.Now);
                }catch(Exception e)
                {
                    count++;
                    _logger.LogError(1, e, "Problema lendo o relógio " + relogio.Nome);
                }
            }
            );

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Terminado ciclo e leitura. Quantidade de erros:" +  count);


            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de Leitura Relógio Terminado.");
            return base.StopAsync(cancellationToken);
        }
    }
}
