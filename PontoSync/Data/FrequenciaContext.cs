using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoSync.Models;

namespace PontoSync.Data
{
    public class FrequenciaContext : DbContext
    {

        public FrequenciaContext(DbContextOptions<FrequenciaContext> options)
            : base(options)
        {
        }

        public DbSet<PontoSync.Models.MarcacaoFrequencia> MarcacaoFrequencia { get; set; }
        public DbSet<PontoSync.Models.Servidor> Servidores { get; set; }

    }
}
