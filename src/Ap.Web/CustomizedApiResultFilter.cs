using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Ap.Web
{
    public class CustomizedApiResultFilter : IResultFilter
    {
        private readonly ApiResultWrapperSettings _apiResultWrapperSettings;

        public CustomizedApiResultFilter(IOptions<ApiResultWrapperSettings> webCommonSettingOptions)
        {
            _apiResultWrapperSettings = webCommonSettingOptions.Value;
        }
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        protected virtual bool NeedToProcess(ResultExecutingContext context)
        {
            //检查是否已在设置中关闭
            if (!_apiResultWrapperSettings.UseCustomizedApiResultFilter)
            {
                return false;
            }

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(_apiResultWrapperSettings.ExcludeUrlPathPrefix)
                && path.StartsWithSegments(_apiResultWrapperSettings.ExcludeUrlPathPrefix))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_apiResultWrapperSettings.IncludeUrlPathPrefix)
                && !path.StartsWithSegments(_apiResultWrapperSettings.IncludeUrlPathPrefix))
            {
                return false;
            }

            //如果在请求的Url中包含noresultwrapping参数，则不对结果进行wrap
            if (context.HttpContext.Request.Query.ContainsKey("noresultwrapping"))
            {
                return false;
            }

            // var objResult = context.Result as ObjectResult;
            // if (null == objResult)
            // {
            //     return false;
            // }

            return true;
        }

        protected virtual void Process(ObjectResult objResult)
        {
            objResult.Value = new ApiResultWrapper()
            {
                Code = "success",
                Message = "",
                Value = objResult.Value
            };
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!this.NeedToProcess(context))
            {
                return;
            }

            if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(null);
            }

            this.Process(context.Result as ObjectResult);
        }
    }
}
