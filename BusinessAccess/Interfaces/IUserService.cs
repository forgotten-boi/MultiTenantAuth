using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Security;
using Entities.Args;

namespace BusinessAccess.Interfaces
{
    public interface IUserService
    {
        Task SignInAsync(TokenRequestArgs args);
        Task<JwtToken> GetTokenAsync(TokenRequestArgs args);
        Task<JwtToken> GetRefreshTokenAsync(TokenRequestArgs args);
        Task<IEnumerable<string>> GetScopesAsync(string userName);
    }
}
