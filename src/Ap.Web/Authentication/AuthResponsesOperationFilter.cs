using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Ap.Web.Authentication
{
    /// <summary>
    /// 给SaggerUI文档自动加上401的Respose
    /// </summary>
    class AuthResponsesOperationFilter : IOperationFilter
    {
        private readonly IOptions<AuthorizationOptions> _authorizationOptions;

        public AuthResponsesOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            _authorizationOptions = authorizationOptions;
        }
        public void Apply(Operation operation, OperationFilterContext context)
        {
            //var controllerPolicies = context.ApiDescription.ControllerAttributes()
            //.OfType<AuthorizeAttribute>()
            //.Select(attr => attr.Policy);
            //var actionPolicies = context.ApiDescription.ActionAttributes()
            //    .OfType<AuthorizeAttribute>()
            //    .Select(attr => attr.Policy);
            //var policies = controllerPolicies.Union(actionPolicies).Distinct();
            //var requiredClaimTypes = policies
            //    .Select(x => _authorizationOptions.Value.GetPolicy(x))
            //    .SelectMany(x => x.Requirements)
            //    .OfType<ClaimsAuthorizationRequirement>()
            //    .Select(x => x.ClaimType);

            //if (requiredClaimTypes.Any())
            //{
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new Response { Description = "Unauthorized" });
            }

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>>
                    {
                    { "Bearer", new []{"Bearer" } }
                    }
                };
            //}
        }
    }
}
