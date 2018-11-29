using Microsoft.AspNetCore.Http;

namespace MultiTenancy
{
    public interface ITenantContext
    {
        IAppTenant GetTenant();
    }

    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IAppTenant GetTenant()
        {
            return _httpContextAccessor.HttpContext.Features.Get<IAppTenant>();
        }
    }
}
