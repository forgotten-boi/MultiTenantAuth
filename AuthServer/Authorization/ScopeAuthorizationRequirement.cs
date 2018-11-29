using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Authorization
{
    public class ScopeAuthorizationRequirement : IAuthorizationRequirement
    {
        public ScopeAuthorizationRequirement(string scope)
        {
            Scope = scope;
        }
        public string Scope { get; set; }
    }
}
