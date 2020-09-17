using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PontoSync.Data;
using PontoSync.Service;
using PontoSync.Service.Cron;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace PontoSync
{
    public class Startup
    {
        public Startup(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = env;
        }
        /// <summary>
        /// https://www.brunobrito.net.br/aspnet-core-identityserver4/
        /// https://stackoverflow.com/questions/41721032/keycloak-client-for-asp-net-core
        /// https://medium.com/@xavier.hahn/asp-net-core-angular-openid-connect-using-keycloak-6437948c008
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public static string PublicClientId { get; private set; }
      
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //options.UseOracleSQLCompatibility("11")
            //Se não colocar esta opção irá gerar sqls que não serão executadas no Oracle 11.

            services.AddDbContext<PontoSyncContext>(options =>
                    options.UseOracle(
                        Configuration.GetConnectionString("PontoSyncContext"), 
                        options => options.UseOracleSQLCompatibility("11")));

            services.AddDbContext<FrequenciaContext>(options =>
                    options.UseOracle(Configuration.GetConnectionString("FrequenciaContext"),
                    options => options.UseOracleSQLCompatibility("11")));

            Boolean isLeituraCronEnabled = Configuration.GetValue<Boolean>("Cron:IsLeituraDigitalCronEnabled",false);
            if (isLeituraCronEnabled)
            {
                services.AddCronJob<LeituraRelogioCronJob>(c =>
                {                    
                    c.TimeZoneInfo = TimeZoneInfo.Local;
                    c.CronExpression = Configuration.GetValue<String>("Cron:LeituraDigitalCron", @"*/10 * * * *");
                });
            }

         
            String serverAddress = Configuration["Authentication:KeycloakAuthentication:ServerAddress"];
            String authorityAddress = serverAddress + "/auth/realms/" + Configuration["Authentication:KeycloakAuthentication:Realm"];
            String audience = Configuration["Authentication:KeycloakAuthentication:ClientId"];

            services.AddAuthentication(options =>
           {
               options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
           })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = authorityAddress;
                    options.ClientId = audience;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Add("openid");
                });
            services.AddAuthorization();

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

            app.UseAuthentication();
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
