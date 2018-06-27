// Copyright 2018 Battelle Energy Alliance, LLC
// ALL RIGHTS RESERVED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreWebApp
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
            string idP = string.Empty;
            try
            {
                idP = Configuration["idP"];
            }
            catch { }
            finally
            {
                if (string.IsNullOrEmpty(idP))
                    idP = "Adfs";
            }

            if (idP == "Adfs")
            {
                services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    var reply = Configuration["Adfs:Reply"];
                    options.Wtrealm = Configuration["Adfs:Realm"];
                    options.MetadataAddress = Configuration["Adfs:Metadata"];
                    options.Wreply = Configuration["Adfs:Reply"];
                    options.CallbackPath = "/";
                    options.SignOutWreply = Configuration["Adfs:Reply"] + "/Account/SignOutCallback";
                    options.SkipUnrecognizedRequests = true;
                    options.Events = new WsFederationEvents
                    {
                        OnSecurityTokenValidated = context =>
                        {
                            string upn = context.Principal.Claims.Where(x => x.Type == ClaimTypes.Upn).First().Value;
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Upn, upn),
                                new Claim(ClaimTypes.Name, upn)
                            };
                            if (context.Principal.Identities.Count() > 0)
                            {
                                context.Principal.Identities.First().AddClaims(claims);
                            }
                            else
                            {
                                var appIdentity = new ClaimsIdentity(claims);
                                context.Principal.AddIdentity(appIdentity);
                            }
                            return Task.FromResult(0);
                        }
                    };
                })
                .AddCookie();
            }


            services.AddMvc();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(12);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
