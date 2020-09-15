using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PontoSync.Data;
using Oracle.ManagedDataAccess.Client;
using PontoSync.Service;
using PontoSync.Service.Cron;

namespace PontoSync
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<PontoSyncContext>(options =>
                    options.UseOracle(
                        Configuration.GetConnectionString("PontoSyncContext"), 
                        options => options.UseOracleSQLCompatibility("11")));

            services.AddDbContext<FrequenciaContext>(options =>
                    options.UseOracle(Configuration.GetConnectionString("FrequenciaContext"),
                    options => options.UseOracleSQLCompatibility("11")));
            if (1==2){
                services.AddCronJob<LeituraRelogioCronJob>(c =>
                {
                    c.TimeZoneInfo = TimeZoneInfo.Local;
                    c.CronExpression = @"*/15 * * * *";
                });
            }
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapAreaControllerRoute(
                       name: "areas",
                       areaName: "myarea",
                           pattern: "{area:exists}/{controller=Home}/{did?}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
