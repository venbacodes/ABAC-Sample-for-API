using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Venbacodes.Samples.ABAC.Authorization
{
    public class VUPolicyProvider : IAuthorizationPolicyProvider
    {
        public VUPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            //This is to skip evaulating other policies if one fails.
            options.Value.InvokeHandlersAfterFailure = false;

            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder("VU").RequireAuthenticatedUser().Build());
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            var policyTokens = policyName.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (policyTokens?.Any() != true)
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            var policy = new AuthorizationPolicyBuilder("VU");

            foreach (var token in policyTokens)
            {
                var pair = token.Split('$', StringSplitOptions.RemoveEmptyEntries);

                if (pair != null && pair.Length == 2)
                {
                    IAuthorizationRequirement requirement = (pair[0]) switch
                    {
                        VUAuthorizeAttribute.RolesGroup => new RolesRequirement(pair[1]),
                        VUAuthorizeAttribute.PermissionsGroup => new PermissionsRequirement(pair[1]),
                        _ => null,
                    };

                    if (requirement == null)
                    {
                        return FallbackPolicyProvider.GetPolicyAsync(policyName);
                    }

                    policy.AddRequirements(requirement);
                }
            }

            return Task.FromResult(policy.Build());
        }
    }
}
