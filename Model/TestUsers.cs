using Venbacodes.Samples.ABAC.Domain;

namespace Venbacodes.Samples.ABAC.API.Model
{
    public class TestUsers
    {
        public static List<VUUser> Users
        {
            get
            {
                return new List<VUUser>
                {
                    new VUUser(1,
                        "Admin",
                        "User",
                        "admin@venbacodes.com",
                        new List<UserRole>
                        {
                            UserRole.Admin

                        },
                        new List<string>
                        {
                            "13",
                            "29"
                        }),
                    new VUUser(2,
                        "Dev",
                        "User",
                        "dev@venbacodes.com",
                        new List<UserRole>
                        {
                            UserRole.Developer

                        },new List<string>
                        {
                            "1",
                            "13",
                            "29"
                        }),
                    new VUUser(3,
                        "End",
                        "User",
                        "enduser@venbacodes.com",
                        new List<UserRole>
                        {
                            UserRole.EndUser
                        },
                        new List<string>
                        {
                            "13"
                        })
                };
            }
        }
    }
}
