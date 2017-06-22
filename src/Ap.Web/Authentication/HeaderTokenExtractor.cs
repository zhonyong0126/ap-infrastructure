using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using Microsoft.Extensions.Logging;

namespace Ap.Web.Authentication
{
    internal class HeaderTokenExtractor : IBearerTokenExtractor
    {
        static readonly string BearerToken = "bearer ";
        private readonly ILogger<HeaderTokenExtractor> _logger;

        public HeaderTokenExtractor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HeaderTokenExtractor>();
        }

        public IBearerTokenExtractor Next { get; set; }

        public async ValueTask<string> GetTokenAsync(HttpRequest request)
        {
            _logger.LogInformation("Attempting to get token from Header.");
            if (!request.Headers.TryGetValue("Authorization", out StringValues values)
                || values.Count == 0)
            {
                _logger.LogInformation("Attempting to get token from Next.");
                if (null == Next)
                {
                    return string.Empty;
                }
                var tmpToken = await Next.GetTokenAsync(request);
                return tmpToken;
            }

            _logger.LogInformation("Parsing.");

            var token = values[0];
            var indexOfBearer = token.IndexOf(BearerToken, StringComparison.OrdinalIgnoreCase);
            if (indexOfBearer > -1)
            {
                var result = token.Substring(indexOfBearer + BearerToken.Length);
                _logger.LogInformation("Complete.");
                return result;
            }

            return string.Empty;
        }
    }
}
