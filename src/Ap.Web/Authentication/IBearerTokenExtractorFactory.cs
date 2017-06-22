using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Web.Authentication
{
    public interface IBearerTokenExtractorFactory
    {
        IBearerTokenExtractor Create();
    }
}
