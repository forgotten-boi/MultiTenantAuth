using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;

namespace BusinessAccess.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<AppTenant>> GetTenantsAsync();
    }
}
