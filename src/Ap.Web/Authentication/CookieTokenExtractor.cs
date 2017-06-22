using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ap.Web.Authentication
{
    class CookieTokenExtractor : IBearerTokenExtractor
    {
        private readonly IBearerTokenExtractor _next;

        public CookieTokenExtractor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CookieTokenExtractor>();
        }

        public IBearerTokenExtractor Next { get; set; }

        static string SessionId = "session_id";
        private readonly ILogger<CookieTokenExtractor> _logger;

        public async ValueTask<string> GetTokenAsync(HttpRequest request)
        {
            _logger.LogInformation("Attempting to get token from cookie.");
            if (!request.Cookies.TryGetValue(SessionId, out string session) || string.IsNullOrEmpty(session))
            {
                _logger.LogInformation("Attempting to get token from Next.");
                if (Next == null)
                {
                    return string.Empty;
                }
                return await Next.GetTokenAsync(request);
            }
            _logger.LogInformation("Complete.");
            return session;
        }
    }
}
