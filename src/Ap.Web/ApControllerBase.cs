using Ap.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ap.Web
{
    public abstract class ApControllerBase : Controller, IOperatorProvider
    {
        private readonly ILogger _logger;

        protected ILogger Logger => _logger;

        protected ApControllerBase(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [NonAction]
        public virtual string GetOperator()
        {
            return User?.Identity?.Name??"Roc";
        }
    }
}
