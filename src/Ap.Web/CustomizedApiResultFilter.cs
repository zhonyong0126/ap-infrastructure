using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Ap.Web
{
    class CustomizedApiResultFilter : IResultFilter
    {
        private readonly ApiResultWrapperSettings _apiResultWrapperSettings;

        public CustomizedApiResultFilter(IOptions<ApiResultWrapperSettings> webCommonSettingOptions)
        {
            _apiResultWrapperSettings = webCommonSettingOptions.Value;
        }
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            //检查是否已在设置中关闭
            if (!_apiResultWrapperSettings.UseCustomizedApiResultFilter)
            {
                return;
            }

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(_apiResultWrapperSettings.ExcludeUrlPathPrefix)
                && path.StartsWithSegments(_apiResultWrapperSettings.ExcludeUrlPathPrefix))
            {
                return;
            }

            if (!string.IsNullOrEmpty(_apiResultWrapperSettings.IncludeUrlPathPrefix)
                && !path.StartsWithSegments(_apiResultWrapperSettings.IncludeUrlPathPrefix))
            {
                return;
            }

            //如果在请求的Url中包含noresultwrapping参数，则不对结果进行wrap
            if (context.HttpContext.Request.Query.ContainsKey("noresultwrapping"))
            {
                return;
            }

            if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(null);
            }

            var objResult = context.Result as ObjectResult;
            if (null == objResult)
            {
                return;
            }

            objResult.Value = new ApiResultWrapper()
            {
                Code = "success",
                Message = "",
                Value = objResult.Value
            };
        }
    }
}
