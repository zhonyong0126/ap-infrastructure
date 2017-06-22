using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ap.Web
{
    class LoggedinAuthroizationHandler : AuthorizationHandler<LoginRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LoginRequirement requirement)
        {
            var playerId = context.User?.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(playerId))
            {
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    class LoginRequirement : IAuthorizationRequirement
    { }
}
