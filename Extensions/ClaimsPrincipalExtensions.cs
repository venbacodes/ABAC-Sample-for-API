using System.Security.Claims;

namespace Venbacodes.Samples.ABAC.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static T GetClaim<T>(this ClaimsPrincipal principal, string claim)
        {
            if (typeof(T) == typeof(List<string>))
            {
                var results = principal?.Claims?.Where(f => f.Type.Equals(claim, StringComparison.OrdinalIgnoreCase)).Select(s => s.Value).ToList();
                return (T)Convert.ChangeType(results, typeof(T));
            }
            else
            {
                var result = principal?.Claims?.FirstOrDefault(f => f.Type.Equals(claim, StringComparison.OrdinalIgnoreCase))?.Value;
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        public static bool HasRole(this ClaimsPrincipal principal, string role)
        {
            return principal.HasClaim(c =>
                                            c.Type == ClaimTypes.Role &&
                                            c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
        }
    }
}
