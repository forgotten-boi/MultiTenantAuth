using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Common.Security;
using DataAccess.Data.Interfaces;
using Entities;
using Entities.Args;
using MultiTenancy;

namespace BusinessAccess.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IAppTenant _tenant;

        private int expiresInMinutes = 5 * 60; //5 hrs
        public UserService(IUserRepository userRepository, 
            ITenantRepository tenantRepository,
            ITenantContext tenantContext)
        {
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _tenant = tenantContext.GetTenant();
        }

        public async Task SignInAsync(TokenRequestArgs args)
        {
            var valid = (_tenant.TenantName == args.client_id && _tenant.APIKey == args.client_secret);
            if (!valid)
                throw new Exception("Tenant is not valid.");

            //validate if user name or password is correct
            var success = await _userRepository.SignInAsync(args.username, args.password);
            if (!success)
                throw new Exception("Invalid username or password.");
        }

        public async Task<JwtToken> GetTokenAsync(TokenRequestArgs args)
        {
            //first validate user name and password
            await SignInAsync(args);

            //store refresh token
            var refreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            await _tenantRepository.AddRefreshTokenAsync(new RefreshToken
            {
                ClientID = args.client_id,
                UserName = args.username,
                Token = refreshToken,
                ExpiresInMinutes = expiresInMinutes
            });
            args.refresh_token = refreshToken;
            return await CreateTokenAsync(args);
        }

        public async Task<JwtToken> GetRefreshTokenAsync(TokenRequestArgs args)
        {
            var token = await _tenantRepository.GetRefreshTokenAsync(args.client_id, args.refresh_token);
            if (token == null)
                throw new Exception("Refresh token is not valid.");

            if (token.Expired)
                throw new Exception("Refresh token is expired.");

            //token is requested before it is expired. so manually generate new one and replace it
            token.ExpiresInMinutes = expiresInMinutes;
            token.Token = Guid.NewGuid().ToString().Replace("-", string.Empty);
            await _tenantRepository.UpdateRefreshTokenAsync(token);

            //user name is required to sign token
            args.username = token.UserName;
            args.refresh_token = token.Token;
            return await CreateTokenAsync(args);
        }

        public Task<IEnumerable<string>> GetScopesAsync(string userName)
        {
            return _userRepository.GetScopesAsync(userName);
        }

        async Task<JwtToken> CreateTokenAsync(TokenRequestArgs args)
        {
            //sign and return access token along with refresh token
            var settings = new JwtTokenSettings
            {
                SigningKey = _tenant.SigningKey,
                Issuer = _tenant.HostName,
                Audience = _tenant.TenantName
            };
            var provider = new JwtTokenProvider(settings);
            var token = await provider.SignAsync(args.username, null);
            token.refresh_token = args.refresh_token;
            return token;
        }
    }
}
