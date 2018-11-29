using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using DataAccess.Data.Interfaces;
using Entities;

namespace BusinessAccess.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }
        public async Task<IEnumerable<AppTenant>> GetTenantsAsync()
        {
            return await _tenantRepository.GetTenantsAsync();
        }
    }
}
