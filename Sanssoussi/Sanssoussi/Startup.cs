using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Sanssoussi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });

            services.AddRazorPages();               // Classes PageModel
            services.AddControllersWithViews();     // Structure Model-Vue-Controller

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = Status307TemporaryRedirect;
                options.HttpsPort = 5001;
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddCors();
            
            services.AddAntiforgery(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Strict; 
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // **TO KEEP**
            //app.Use(async (context, next) =>
            //{
            //    // Supprimer tous les cookies
            //    foreach (var cookie in context.Request.Cookies.Keys)
            //    {
            //        context.Response.Cookies.Delete(cookie);
            //    }

            //    await next();
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection(); //Permet de rediriger les requ�tes HTTP vers HTTPS

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication(); // Cookie d'authentification pour maintenir la connection

            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapRazorPages();
                });
        }
    }
}