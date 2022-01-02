using Venbacodes.Samples.ABAC.Domain;
using static Venbacodes.Samples.ABAC.Domain.Permissions;

namespace Venbacodes.Samples.ABAC.API.Model
{
    public class VUUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public IEnumerable<UserRole> Roles { get; protected set; }
        public IEnumerable<UserPermission> Permissions { get; protected set; }
        public IEnumerable<string> Categories { get; set; }

        public VUUser(int userID, string firstName, string lastName, string email, IEnumerable<UserRole> roles, IEnumerable<string> categories)
        {
            Id = userID;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Roles = roles;
            Categories = categories;
            Permissions = roles.SelectMany(r => GetPermissions(r));
        }

        private IEnumerable<UserPermission> GetPermissions(UserRole userRole)
        {
            var permissions = new List<UserPermission>();

            switch (userRole)
            {
                case UserRole.Developer:
                case UserRole.Admin:
                    permissions.Add(new UserPermission(Access.View, Scope.All, Module.Category));
                    permissions.Add(new UserPermission(Access.View, Scope.Self, Module.Category));
                    permissions.Add(new UserPermission(Access.View, Scope.All, Module.Division));                    
                    permissions.Add(new UserPermission(Access.View, Scope.Self, Module.Division));
                    break;

                case UserRole.EndUser:
                    permissions.Add(new UserPermission(Access.View, Scope.Self, Module.Category));
                    permissions.Add(new UserPermission(Access.View, Scope.Self, Module.Division));
                    break;
            }

            return permissions;
        }
    }
}
