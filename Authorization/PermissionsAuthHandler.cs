using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Venbacodes.Samples.ABAC.Domain;

namespace Venbacodes.Samples.ABAC.Authorization
{
    public class PermissionsAuthHandler : AuthorizationHandler<PermissionsRequirement>
    {
        private readonly ILogger<PermissionsAuthHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string Categories = "Categories";
        private const string CategoryId = "CategoryId";
        private const string Permission = "Permission";

        public PermissionsAuthHandler(ILogger<PermissionsAuthHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionsRequirement requirement)
        {
            var userEmail = "";
            try
            {
                if (!context.User.Identity.IsAuthenticated || context.HasSucceeded || requirement == null || string.IsNullOrWhiteSpace(requirement.Permissions))
                {
                    return Task.CompletedTask;
                }

                userEmail = context.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Email).Value;

                var expectedPermissions = GetPermissions(requirement.Permissions.Split("|", StringSplitOptions.RemoveEmptyEntries));

                var userPermissionClaims = context.User.Claims.
                                                Where(c => c.Type.Equals(Permission, StringComparison.OrdinalIgnoreCase)).
                                                Select(s => s.Value);

                var actualPermissions = GetPermissions(userPermissionClaims);

                foreach (var _expectedPermission in expectedPermissions)
                {
                    var _actualPermission = actualPermissions.
                                                Where(w => w.Access == _expectedPermission.Access &&
                                                        w.Scope == _expectedPermission.Scope &&
                                                        w.Module == _expectedPermission.Module);

                    if (!_actualPermission.Any())
                    {
                        context.Fail();
                    }
                }

                //this is an optional additional check
                if (!context.HasFailed &&
                    expectedPermissions.Any(f =>
                                                f.Access == Permissions.Access.View &&
                                                f.Scope == Permissions.Scope.Self &&
                                                f.Module == Permissions.Module.Category) &&
                    !actualPermissions.Any(f =>
                                                f.Access == Permissions.Access.View &&
                                                f.Scope == Permissions.Scope.All &&
                                                f.Module == Permissions.Module.Category))
                {
                    var userCategories = context.User.Claims.
                                                Where(c => c.Type.Equals(Categories, StringComparison.OrdinalIgnoreCase));

                    var requestedCategoryId = _httpContextAccessor.HttpContext.GetRouteValue(CategoryId).ToString();

                    if (!string.IsNullOrWhiteSpace(requestedCategoryId) &&
                        !userCategories.Any(a => a.Value.Equals(requestedCategoryId, StringComparison.OrdinalIgnoreCase)))
                    {
                        //Fail the requirement as data requested for Category Id does not match with the user's Category Id
                        context.Fail();
                    }
                    else
                    {
                        //this is an optional way to restrict the user to access their own resources by changing their request itself
                        //set user's CategoryId if there is none
                        var queryItems = _httpContextAccessor.HttpContext.Request.Query.
                                            Where(w => !string.IsNullOrWhiteSpace(w.Key) && !string.IsNullOrWhiteSpace(w.Value)).
                                            ToDictionary(k => k.Key, v => v.Value.ToString());

                        queryItems.Add(CategoryId, userCategories.FirstOrDefault().Value);

                        _httpContextAccessor.HttpContext.Request.QueryString = new QueryBuilder(queryItems.Select(s => s)).ToQueryString();

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
                context.Fail();
                _logger.LogError(ex, $"Error in handling permission requirement");
            }

            if (context.HasFailed)
            {
                _logger.LogTrace($"Permission requirement not succedded for {userEmail}");
                SetResponseContext(_httpContextAccessor.HttpContext);
            }
            else
            {
                _logger.LogTrace($"Permission requirement succedded for {userEmail}");
            }

            return Task.CompletedTask;
        }

        private static List<UserPermission> GetPermissions(IEnumerable<string> permissions)
        {
            var userPermissions = new List<UserPermission>();

            foreach (var permission in permissions)
            {
                var separatedTokens = permission.Split(".");

                userPermissions.Add(
                    new UserPermission(
                        Enum.Parse<Permissions.Access>(separatedTokens[0]),
                        Enum.Parse<Permissions.Scope>(separatedTokens[1]),
                        Enum.Parse<Permissions.Module>(separatedTokens[2])));
            }

            return userPermissions;
        }

        private void SetResponseContext(HttpContext httpContext)
        {
            if (!httpContext.Response.HasStarted)
            {
                var response = new
                {
                    Id = Guid.NewGuid().ToString(),
                    CorrelationId = Activity.Current?.Id,
                    StatusCode = HttpStatusCode.Forbidden.ToString(),
                    Message = "You don't have permissions to access this endpoint in a manner you trying to access it"
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
                _logger.LogDebug($"Starting response writing in {typeof(PermissionsAuthHandler)}");
                httpContext.Response.WriteAsync(message);
            }
        }
    }
}
