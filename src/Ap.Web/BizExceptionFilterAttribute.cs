using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ap.Infrastructure;

namespace Ap.Web
{
    class BizExceptionFilterAttribute : ExceptionFilterAttribute
    {

        private readonly IModelMetadataProvider _modelMetadataProvider;
        public BizExceptionFilterAttribute(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            var bizException = context.Exception as BizException;
            if (bizException == null)
            {
                return;
            }

            context.ExceptionHandled = true;
            context.Result = new ObjectResult(new ApiResultWrapper() { Code = bizException.Code, Message = bizException.Message });
        }
    }
}
