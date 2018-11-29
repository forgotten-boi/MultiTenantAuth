using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> SignInAsync(string userName, string password);
        Task<IEnumerable<string>> GetScopesAsync(string userName);
    }
}
