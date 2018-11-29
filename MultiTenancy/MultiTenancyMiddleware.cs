using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using System.Net;

namespace MultiTenancy
{
    public class MultiTenancyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Func<HttpContext, Task<IAppTenant>> _tenantResolver;
        public MultiTenancyMiddleware(RequestDelegate next, Func<HttpContext, Task<IAppTenant>> tenantResolver)
        {
            _next = next;
            _tenantResolver = tenantResolver;
        }

        public async Task Invoke(HttpContext context)
        {
            var tenant = await _tenantResolver(context);
            if (tenant == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Tenant is not valid.");
                return;
            }
            context.Features.Set(tenant);
            await _next.Invoke(context);
        }
    }

    public static class MultiTenancyMiddlewareExtension
    {
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app, Func<HttpContext, Task<IAppTenant>> tenantResolver)
        {
            app.UseMiddleware<MultiTenancyMiddleware>(tenantResolver);
            return app;
        }
    }
}
