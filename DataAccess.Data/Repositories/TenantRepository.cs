using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Abstract;
using DataAccess.Data.Interfaces;
using DataAccess.Interfaces;
using Entities;

namespace DataAccess.Data.Repositories
{
    public class TenantRepository : Repository<AppTenant>, ITenantRepository
    {
        public TenantRepository(IDatabase dbContext) : base(dbContext)
        {
        }

        public async Task<bool> AddRefreshTokenAsync(RefreshToken token)
        {
            const string query = @"DECLARE @Now DATETIME = GETUTCDATE();
                                INSERT INTO RefreshToken(ClientID, UserName, Token, CreatedOn, ExpiresOn)
                                VALUES(@ClientID, @UserName, @Token, @Now, DATEADD(MINUTE, @ExpiresInMinutes, @Now))";
            await DbContext.ExecuteNonQueryAsync(query, args =>
            {
                args.Add("@ClientID", token.ClientID);
                args.Add("@UserName", token.UserName);
                args.Add("@Token", token.Token);
                args.Add("@ExpiresInMinutes", token.ExpiresInMinutes);
            });
            return true;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string clientID, string token)
        {
            const string query = @"DECLARE @Now DATETIME = GETUTCDATE();
                                SELECT 
	                                TokenID, 
	                                ClientID, 
                                    UserName,
	                                Token, 
	                                Expired = CAST((CASE WHEN @Now < CreatedOn AND @Now > ExpiresOn THEN 1 ELSE 0 END) AS BIT)
                                FROM RefreshToken
                                WHERE ClientID = @ClientID 
                                AND Token = @Token";
            var items = await DbContext.ExecuteReaderAsync(query, args =>
            {
                args.Add("@ClientID", clientID);
                args.Add("@Token", token);
            }, reader => new RefreshToken
            {
                TokenID = reader.Get<int>(0),
                ClientID = reader.Get<string>(1),
                UserName = reader.Get<string>(2),
                Token = reader.Get<string>(3),
                Expired = reader.Get<bool>(4)
            });
            return items.FirstOrDefault();
        }

        public async Task<bool> UpdateRefreshTokenAsync(RefreshToken token)
        {
            const string query = @"UPDATE RefreshToken SET 
                                    Token = @Token,
                                    ExpiresOn = DATEADD(MINUTE, @ExpiresInMinutes, GETUTCDATE()), 
                                    ModifiedOn = GETUTCDATE()
                                WHERE TokenID = @TokenID";
            await DbContext.ExecuteNonQueryAsync(query, args =>
            {
                args.Add("@Token", token.Token);
                args.Add("@ExpiresInMinutes", token.ExpiresInMinutes);
                args.Add("@TokenID", token.TokenID);
            });
            return true;
        }


        public async Task<IEnumerable<AppTenant>> GetTenantsAsync()
        {
            const string query = @"SELECT T.[TenantID]
                                        ,T.[TenantName]
                                        ,T.[APIKey]
                                        ,T.[HostName]
                                        ,T.[DatabaseName]
                                        ,DF.[FormatExpr]
                                        ,T.SigningKey
                                FROM [Tenant] T
                                JOIN [DateFormat] DF ON DF.DateFormatID = T.DateFormatID
                                WHERE T.Active = 1
                                AND DF.Active = 1";
            var items = await DbContext.ExecuteReaderAsync(query, args => { }, reader => new AppTenant
            {
                TenantID = reader.Get<int>(0),
                TenantName = reader.Get<string>(1),
                APIKey = reader.Get<string>(2),
                HostName = reader.Get<string>(3),
                DatabaseName = reader.Get<string>(4),
                DateFormat = reader.Get<string>(5),
                SigningKey = reader.Get<string>(6)
            });
            return items.ToList();
        }
        public async Task<AppTenant> GetTenantAsync(string tenantName, string apiKey)
        {
            const string query = @"SELECT T.[TenantID]
                                        ,T.[TenantName]
                                        ,T.[APIKey]
                                        ,T.[HostName]
                                        ,T.[DatabaseName]
                                        ,DF.[FormatExpr]
                                        ,T.SigningKey
                                FROM [Tenant] T
                                JOIN [DateFormat] DF ON DF.DateFormatID = T.DateFormatID
                                WHERE T.Active = 1
                                AND DF.Active = 1
                                AND T.TenantName = @TenantName
                                AND T.APIKey = @APIKey";
            var items = await DbContext.ExecuteReaderAsync(query, args =>
            {
                args.Add("@TenantName", tenantName);
                args.Add("@APIKey", apiKey);
            }, reader => new AppTenant
            {
                TenantID = reader.Get<int>(0),
                TenantName = reader.Get<string>(1),
                APIKey = reader.Get<string>(2),
                HostName = reader.Get<string>(3),
                DatabaseName = reader.Get<string>(4),
                DateFormat = reader.Get<string>(5),
                SigningKey = reader.Get<string>(6)
            });
            return items.FirstOrDefault();
        }
    }
}
