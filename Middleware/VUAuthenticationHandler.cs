using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using Venbacodes.Samples.ABAC.API.Extensions;
using Venbacodes.Samples.ABAC.API.Model;
using Venbacodes.Samples.ABAC.Domain;

namespace Venbacodes.Samples.ABAC.Middleware
{
    public class VUAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ILogger<VUAuthenticationHandler> _logger;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        private const string Authorization = "Authorization";
        private const string Bearer = "Bearer";
        private const string Categories = "Categories";
        private const string Permission = "Permission";
        private const string AuthenticationToken = "AuthenticationToken";
        private readonly string _expClaim = "exp";
        private readonly IEnumerable<string> _emailClaims = new string[] { "email", "sub" };

        public VUAuthenticationHandler(
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler ?? throw new ArgumentNullException(nameof(jwtSecurityTokenHandler));
            _logger = logger.CreateLogger<VUAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var activityId = Activity.Current?.Id;
            var message = "";

            try
            {
                var authenticationToken = Request.Headers[Authorization].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(authenticationToken))
                {
                    _logger.LogDebug($"Authentication token found in request and starting validation - ActivityId {activityId }");

                    authenticationToken = authenticationToken.Substring((Bearer + " ").Length).Trim();

                    var jwtToken = (JwtSecurityToken)_jwtSecurityTokenHandler.ReadToken(authenticationToken);

                    //Extract details
                    var email = jwtToken.GetTokenData<string>(_emailClaims);
                    var expUnixTimeStamp = jwtToken.GetTokenData<long>(_expClaim);

                    //Finding date time from UNIX time
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTimeStamp);

                    //check if the authtime is expired
                    if (expTime < DateTimeOffset.UtcNow)
                    {
                        message += $"Authentication token sent for {email} has expired";
                        _logger.LogDebug(message);
                        return await Task.FromResult(AuthenticateResult.Fail(message));
                    }

                    var vuUser = TestUsers.Users.FirstOrDefault(w => w.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                    if (vuUser != null)
                    {
                        var genericIdentity = new GenericIdentity("VUID", "VU");

                        genericIdentity.AddClaim(new Claim(ClaimTypes.Name, vuUser.FirstName + " " + vuUser.LastName));
                        genericIdentity.AddClaim(new Claim(ClaimTypes.Email, vuUser.Email));
                        genericIdentity.AddClaims(vuUser.Categories.Select(s => new Claim(Categories, s.ToString())));
                        genericIdentity.AddClaims(vuUser.Roles.Select(s => new Claim(ClaimTypes.Role, s.ToString())));
                        genericIdentity.AddClaims(vuUser.Permissions.Select(s => new Claim(Permission, $"{s.Access}.{s.Scope}.{s.Module}")));

                        var claimsPrincipal = new ClaimsPrincipal(genericIdentity);

                        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                        message += $"Validation success for the token in the request - ActivityId {activityId }";
                        _logger.LogDebug(message);

                        Context.Items[AuthenticationToken] = authenticationToken;
                        return await Task.FromResult(AuthenticateResult.Success(ticket));

                    }

                    message += $"Validation failure for the token in the request - ActivityId {activityId } ";
                }
                else
                {
                    message += $"Authorization header is needed - ActivityId {activityId }";
                }

                _logger.LogDebug(message);
                return await Task.FromResult(AuthenticateResult.Fail(message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token validation error - ActivityId {activityId }");
                return await Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            //Avoiding setting status code in this virtual method as we have added response in requirement auth handlers
            return Task.CompletedTask;
        }
    }
}