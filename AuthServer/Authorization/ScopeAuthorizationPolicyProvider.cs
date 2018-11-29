using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorization
{
    public class ScopeAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public ScopeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }
        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Check static policies first
            var policy = await base.GetPolicyAsync(policyName);
            if (policy == null)
            {
                policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ScopeAuthorizationRequirement(policyName))
                    .Build();
            }
            return policy;
        }
    }
}
