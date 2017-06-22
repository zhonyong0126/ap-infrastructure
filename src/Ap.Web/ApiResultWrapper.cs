using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Web
{
    public class ApiResultWrapper
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Value { get; set; }
    }
}
