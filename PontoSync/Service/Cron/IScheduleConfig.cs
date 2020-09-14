using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoSync.Service.Cron
{
    public interface IScheduleConfig<T>
    {
        string CronExpression { get; set; }
        TimeZoneInfo TimeZoneInfo { get; set; }
    }
}
