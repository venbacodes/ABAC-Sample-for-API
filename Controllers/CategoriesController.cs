using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Venbacodes.Samples.ABAC.Authorization;
using Venbacodes.Samples.ABAC.Domain;
using Venbacodes.Samples.ABAC.Extensions;

namespace Venbacodes.Samples.ABAC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        public List<string> Categories { get; set; }
        public IEnumerable<string> MyCategories { get; set; }

        public CategoriesController()
        {
            Categories = new List<string>()
            {
                "13",
                "29"
            };
        }

        [VUAuthorize(new UserRole[] { UserRole.Admin }, new string[] { BuiltPermissions.ViewAllCategory })]
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Categories;
        }

        [VUAuthorize(new UserRole[] { UserRole.Admin, UserRole.EndUser, UserRole.Developer }, new string[] { BuiltPermissions.ViewSelfCategory })]
        [HttpGet("{categoryId}")]
        public IEnumerable<string> Get(int categoryId)
        {
            if (!User.HasRole("Admin"))
            {
                var userCategories = User.GetClaim<List<string>>("Categories");

                MyCategories = Categories.Intersect(userCategories);
            }
            else
            {
                MyCategories = Categories;
            }

            return MyCategories.Where(w => w.Equals(categoryId.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
