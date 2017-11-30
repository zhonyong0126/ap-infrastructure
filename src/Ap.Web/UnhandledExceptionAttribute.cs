using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Ap.Web
{
    public class UnhandledExceptionAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<UnhandledExceptionAttribute> logger;
        public UnhandledExceptionAttribute(ILogger<UnhandledExceptionAttribute> logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            this.logger.LogCritical(context.Exception, $"RequestId:{context.HttpContext.Connection.Id},{context.Exception.Message}");
        }
    }
}