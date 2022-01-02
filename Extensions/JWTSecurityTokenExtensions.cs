using System.IdentityModel.Tokens.Jwt;
using Venbacodes.Samples.ABAC.API.Model;
using Venbacodes.Samples.ABAC.Domain;

namespace Venbacodes.Samples.ABAC.API.Extensions
{
    public static class JWTSecurityTokenExtensions
    {
        public static T GetTokenData<T>(this JwtSecurityToken jwtSecurityToken, string claim)
        {
            var result = jwtSecurityToken?.Claims?.FirstOrDefault(f => claim.Equals(f.Type, StringComparison.OrdinalIgnoreCase))?.Value;
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public static T GetTokenData<T>(this JwtSecurityToken jwtSecurityToken, IEnumerable<string> claims)
        {
            var result = jwtSecurityToken?.Claims?.FirstOrDefault(f => claims.Contains(f.Type?.ToLowerInvariant()))?.Value;
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}
