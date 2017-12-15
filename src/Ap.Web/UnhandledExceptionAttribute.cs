using Ap.Infrastructure;
using Microsoft.AspNetCore.Mvc;
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
            if (context.Exception is BizException)
            {
                return;
            }
            context.ExceptionHandled=true;
            context.Result=new StatusCodeResult(500);
            this.logger.LogCritical(context.Exception, $"TraceIdentifier:{context.HttpContext.TraceIdentifier},RequestId:{context.HttpContext.Connection.Id},{context.Exception.Message}");
        }
    }
}