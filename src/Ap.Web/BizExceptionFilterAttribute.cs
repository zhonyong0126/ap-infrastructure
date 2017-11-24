﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ap.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Ap.Web
{
    public class BizExceptionFilterAttribute : ExceptionFilterAttribute
    {

        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ILogger<BizExceptionFilterAttribute> logger;

        public BizExceptionFilterAttribute(IModelMetadataProvider modelMetadataProvider, ILogger<BizExceptionFilterAttribute> logger)
        {
            _modelMetadataProvider = modelMetadataProvider;
            this.logger = logger;
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

            this.logger.LogWarning($"RequestId:{context.HttpContext.Connection.Id},{bizException.Message}");
        }
    }
}
