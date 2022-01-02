using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace Venbacodes.Samples.ABAC.Authorization
{
    public class RolesAuthHandler : AuthorizationHandler<RolesRequirement>
    {
        private readonly ILogger<RolesAuthHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolesAuthHandler(ILogger<RolesAuthHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RolesRequirement requirement)
        {
            var requirementPassed = false;

            try
            {
                if (!context.User.Identity.IsAuthenticated || context.HasSucceeded || requirement == null || string.IsNullOrWhiteSpace(requirement.Roles))
                {
                    return Task.CompletedTask;
                }

                var userEmail = context.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Email).Value;

                var expectedRoles = requirement.Roles.Split("|", StringSplitOptions.RemoveEmptyEntries);

                if (expectedRoles.Any())
                {
                    var actualRoles = context.User.Claims?.Where(c => c.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)).Select(s => s.Value);

                    if (actualRoles.Intersect(expectedRoles).Any())
                    {
                        requirementPassed = true;
                        _logger.LogTrace($"Roles requirement succedded for {userEmail}");
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    context.Succeed(requirement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in handling role requirement");
            }

            if (!requirementPassed)
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (!httpContext.Response.HasStarted)
                {
                    var response = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        CorrelationId = Activity.Current?.Id,
                        StatusCode = HttpStatusCode.Forbidden.ToString(),
                        Message = "Your role doesn't allow you to access this endpoint in a manner you trying to access it"
                    };

                    var message = JsonConvert.SerializeObject(
                                            response,
                                            new JsonSerializerSettings()
                                            {
                                                NullValueHandling = NullValueHandling.Ignore,
                                                DefaultValueHandling = DefaultValueHandling.Ignore
                                            });

                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    httpContext.Response.ContentType = "application/json";
                    _logger.LogDebug($"Starting response writing in {typeof(RolesAuthHandler)}");
                    httpContext.Response.WriteAsync(message);
                }

                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
