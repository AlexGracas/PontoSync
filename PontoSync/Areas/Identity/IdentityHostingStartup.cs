using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PontoSync.Areas.Identity.Data;
using PontoSync.Data;

[assembly: HostingStartup(typeof(PontoSync.Areas.Identity.IdentityHostingStartup))]
namespace PontoSync.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<PontoSyncIdentityContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("PontoSyncIdentityContextConnection")));
                services.AddDefaultIdentity<PontoSyncUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<PontoSyncIdentityContext>();
            });
        }
    }
}