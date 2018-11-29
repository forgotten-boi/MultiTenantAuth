using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using MultiTenancy;

namespace AuthServer.Authorization
{
    public class ScopeAuthorizationHandler : AuthorizationHandler<ScopeAuthorizationRequirement>
    {
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;
        private readonly ITenantContext _tenantContext;
        public ScopeAuthorizationHandler(IUserService userService, IMemoryCache cache, ITenantContext tenantContext)
        {
            _userService = userService;
            _cache = cache;
            _tenantContext = tenantContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ScopeAuthorizationRequirement requirement)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));

            if (context.User.Identity.IsAuthenticated)
            {
                if (await AuthorizeAsync(context.User, requirement.Scope))
                    context.Succeed(requirement);
            }
            await Task.CompletedTask;
        }

        private async Task<bool> AuthorizeAsync(IPrincipal user, string scope)
        {
            //validate user and its scope from database or cache
            var tenant = _tenantContext.GetTenant();
            var cacheKey = $"SCOPES_{tenant.TenantID}_{user.Identity.Name}".Trim().ToUpper();
            var scopes = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(20);
                //get all scopes of a user from database if it doesn't exists in cache.
                return await _userService.GetScopesAsync(user.Identity.Name);
            });
            var authorized = scopes.Any(o => o == scope);
            return await Task.FromResult(authorized);
        }
    }
}