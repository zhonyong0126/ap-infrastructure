using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Ap.Web.Authentication
{
    class BearerTokenExtractorFactory : IBearerTokenExtractorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public BearerTokenExtractorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IBearerTokenExtractor Create()
        {
            var extractor1 = _serviceProvider.GetService<HeaderTokenExtractor>();
            var extractor2 = _serviceProvider.GetService<CookieTokenExtractor>();
            var extractor3 = _serviceProvider.GetService<QueryStringTokenExtractor>();
            extractor2.Next = extractor3;
            extractor1.Next = extractor2;
            return extractor1;
        }
    }
}
