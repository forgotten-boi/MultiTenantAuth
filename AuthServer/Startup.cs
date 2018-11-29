using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using AuthServer.Authorization;
using BusinessAccess.Interfaces;
using BusinessAccess.Services;
using Common.Security;
using DataAccess.Data.Interfaces;
using DataAccess.Data.Repositories;
using DataAccess.Databases;
using DataAccess.Interfaces;
using Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MultiTenancy;
using MultiTenancy.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AuthServer
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
            services.AddMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            
            //add database based localization
            services.AddSqlLocalization(options =>
            {
                var conString = Configuration["ConnectionStrings:ConnectionString"];
                options.ConnectionString = conString;
            });

            //Enable JWTToken validation and Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearerPerTenant((options, tenant) =>
                {
                    var settings = new JwtTokenSettings
                    {
                         SigningKey = tenant.SigningKey,
                         Issuer = tenant.HostName,
                         Audience = tenant.TenantName
                    };
                    var provider = new JwtTokenProvider(settings);
                    options.TokenValidationParameters = provider.GetTokenValidationParameters();
                });

            //Enable Scope based authorization
            services.AddAuthorization();
            services.AddSingleton<IAuthorizationPolicyProvider, ScopeAuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();

            services.AddMvc().AddJsonOptions(options =>
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ne-NP")
                };
                options.DefaultRequestCulture = new RequestCulture(supportedCultures.First());
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            //Register repository and business services
            Register(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env,
            IOptions<RequestLocalizationOptions> options, 
            IMemoryCache cache, 
            ITenantService tenantService)
        {
            #region Global Exception Handling

            app.UseExceptionHandler(errApp =>
            {
                errApp.Run(async context =>
                {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var ex = error.Error;
                        var err = new
                        {
                            ErrorType = ex.GetType().Name,
                            ErrorMessage = ex.Message
                        };
                        var errorMessage = JsonConvert.SerializeObject(err, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                        await context.Response.WriteAsync(errorMessage, Encoding.UTF8);
                    }
                });
            });

            #endregion

            app.UseMultiTenancy(async context =>
            {
                //get all valid tenants from cache or database
                var tenants = await cache.GetOrCreateAsync("TENANTS", async entry =>
                {
                    entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60);
                    return await tenantService.GetTenantsAsync();
                });
                //validate and return tenant if valid
                var hostName = context.Request.Host.Value;
                return tenants.FirstOrDefault(t => t.HostName == hostName);
            });
            //localization from request url
            app.UseRequestLocalization(options.Value);

            app.UseAuthentication();

            app.UseMvc();
        }

        private void Register(IServiceCollection services)
        {
            var conString = Configuration["ConnectionStrings:ConnectionString"];
            services.AddSingleton<ITenantContext, TenantContext>();
            services.AddTransient<IDatabase, SqlDatabase>(args => new SqlDatabase(conString));
            services.AddTransient<ITenantDatabase, TenantDatabase>(args =>
            {
                var tenantContext = args.GetService<ITenantContext>();
                return new TenantDatabase(tenantContext, conString);
            });
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<ITenantService, TenantService>();
        }
    }
}
