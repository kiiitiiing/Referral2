using System;
using System.Globalization;
using System.Security.Claims;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Services;

namespace Referral2
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
            services.AddSingleton(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddDbContext<ReferralDbContext>(options =>
                options/*.UseLazyLoadingProxies()
                        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning))*/
                        .UseSqlServer(Configuration.GetConnectionString("ReferralConnection")));

            services.AddCors();

            services.Configure<ReferralRoles>(Configuration);
            services.Configure<ReferralStatus>(Configuration);
            services.Configure<TrackingType>(Configuration);

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddTransient<IUserService, UserService>();
            services.AddScoped<ICompute, Compute>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/NotFound";
                options.ExpireTimeSpan = TimeSpan.FromHours(3);
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", polBuilder => polBuilder.RequireClaim(ClaimTypes.Role, "admin"));
                options.AddPolicy("Doctor", polBuilder => polBuilder.RequireClaim(ClaimTypes.Role, "doctor"));
                options.AddPolicy("Support", polBuilder => polBuilder.RequireClaim(ClaimTypes.Role, "support"));
                options.AddPolicy("MCC", polBuilder => polBuilder.RequireClaim(ClaimTypes.Role, "mcc"));
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });


            services.AddMvc()
                        .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0)
                        .AddNewtonsoftJson();

            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            services.AddSession(options=> 
            {
                options.Cookie.HttpOnly = true;
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.IsEssential = true;
            });

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

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Admin}/{action=AdminDashboard}");
            });
        }
    }
}
