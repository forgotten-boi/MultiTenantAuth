using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;

namespace DataAccess.Data.Interfaces
{
    public interface ITenantRepository
    {
        Task<bool> AddRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken> GetRefreshTokenAsync(string clientID, string token);
        Task<bool> UpdateRefreshTokenAsync(RefreshToken token);

        Task<IEnumerable<AppTenant>> GetTenantsAsync();
        Task<AppTenant> GetTenantAsync(string tenantName, string apiKey);
    }
}
