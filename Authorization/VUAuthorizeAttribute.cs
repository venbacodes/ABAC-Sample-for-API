using Microsoft.AspNetCore.Authorization;
using Venbacodes.Samples.ABAC.Domain;

namespace Venbacodes.Samples.ABAC.Authorization
{
    public sealed class VUAuthorizeAttribute : AuthorizeAttribute
    {
        public const string RolesGroup = "Roles";
        public const string PermissionsGroup = "Permissions";

        public UserRole[] UserRoles { get; set; }

        public string[] UserPermissions { get; set; }

        public VUAuthorizeAttribute(UserRole[] roles, string[] userPermissions)
        {
            UserRoles = roles;
            UserPermissions = userPermissions;

            //Build policy for roles group
            Policy += $"{RolesGroup}${string.Join("|", UserRoles)};";

            //Build policy for permissions group
            Policy += $"{PermissionsGroup}${string.Join("|", UserPermissions)};";
        }
    }
}
