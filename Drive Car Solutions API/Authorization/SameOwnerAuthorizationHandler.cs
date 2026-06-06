using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Drive_Car_Solutions_API.Models;

namespace Drive_Car_Solutions_API.Authorization
{
    public class SameOwnerAuthorizationHandler : AuthorizationHandler<SameOwnerRequirement, IOwnedResource>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameOwnerRequirement requirement, IOwnedResource resource)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Allow if the user is the owner or is an Admin
            if (resource.ApplicationUserId == userId || context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
