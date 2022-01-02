using Microsoft.AspNetCore.Authorization;

namespace Venbacodes.Samples.ABAC.Authorization
{
    public class PermissionsRequirement : IAuthorizationRequirement
    {
        public string Permissions { get; }

        public PermissionsRequirement(string permissions)
        {
            Permissions = permissions;
        }
    }
}
