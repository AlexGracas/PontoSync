using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PontoSync.Models;

namespace PontoSync.Data
{
    public class PontoSyncContext : DbContext
    {
        public PontoSyncContext (DbContextOptions<PontoSyncContext> options)
            : base(options)
        {
        }

        public DbSet<PontoSync.Models.Relogio> Relogios { get; set; }

        public DbSet<PontoSync.Models.Registro> Registros { get; set; }
    }
}
