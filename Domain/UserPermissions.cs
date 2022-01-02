using static Venbacodes.Samples.ABAC.Domain.Permissions;

namespace Venbacodes.Samples.ABAC.Domain
{
    public class UserPermission
    {
        public Access Access { get; protected set; }
        public Scope Scope { get; protected set; }
        public Module Module { get; protected set; }

        public UserPermission(Access access, Scope scope, Module module)
        {
            Access = access;
            Scope = scope;
            Module = module;
        }
    }
}