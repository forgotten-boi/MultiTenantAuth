using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Abstract;
using DataAccess.Data.Interfaces;

namespace DataAccess.Data.Repositories
{
    public class UserRepository : Repository<object>, IUserRepository
    {
        public UserRepository(ITenantDatabase dbContext) : base(dbContext)
        {
        }

        public async Task<bool> SignInAsync(string userName, string password)
        {
            const string query = @"SELECT AppUserID, UserName 
                                FROM AppUser
                                WHERE Active = 1
                                AND UserName = @UserName
                                AND [Password] = @Password";
            var items = await DbContext.ExecuteReaderAsync(query, args =>
            {
                args.Add("@UserName", userName);
                args.Add("@Password", password);
            }, reader => new 
            {
                AppUserID = reader.Get<int>(0),
                UserName = reader.Get<string>(1)
            });
            return items.Any();
        }

        public async Task<IEnumerable<string>> GetScopesAsync(string userName)
        {
            const string query = @"SELECT 
	                                T2.AppScopeName
                                FROM AppUserScope T1
                                JOIN AppScope T2 ON T2.AppScopeID = T1.AppScopeID
                                WHERE T2.Active = 1
                                AND T1.UserName = @UserName";
            return await DbContext.ExecuteReaderAsync(query,
                args => args.Add("@UserName", userName),
                reader => reader.Get<string>(0));
        }
    }
}
