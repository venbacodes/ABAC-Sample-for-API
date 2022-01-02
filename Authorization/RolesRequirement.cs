using Microsoft.AspNetCore.Authorization;

namespace Venbacodes.Samples.ABAC.Authorization
{
    public class RolesRequirement : IAuthorizationRequirement
    {
        public string Roles { get; }

        public RolesRequirement(string roles)
        {
            Roles = roles;
        }
    }
}
