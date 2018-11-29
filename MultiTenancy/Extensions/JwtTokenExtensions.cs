using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Extensions
{
    public static class JwtTokenExtensions
    {
        public static AuthenticationBuilder AddJwtBearerPerTenant(this AuthenticationBuilder builder,
            Action<JwtBearerOptions, IAppTenant> configureOptions)
        {
            builder.AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        string authorization = ctx.Request.Headers["Authorization"];
                        // If no authorization header found, nothing to process further
                        if (string.IsNullOrWhiteSpace(authorization))
                        {
                            return Task.CompletedTask;
                        }
                        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Token = authorization.Substring("Bearer ".Length).Trim();
                        }
                        // If no token found, no further work possible
                        if (string.IsNullOrWhiteSpace(ctx.Token))
                        {
                            return Task.CompletedTask;
                        }
                        //if token is present, alter tokenvalidation parameters for each tenant
                        if (!string.IsNullOrWhiteSpace(ctx.Token))
                        {
                            var tenantContext = ctx.HttpContext.RequestServices.GetService<ITenantContext>();
                            configureOptions(options, tenantContext.GetTenant());
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            return builder;
        }
    }
}
