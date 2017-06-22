using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Infrastructure
{
    public abstract class BizException : Exception
    {
        protected BizException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }
}
