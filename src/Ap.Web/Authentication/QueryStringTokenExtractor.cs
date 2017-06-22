using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;

namespace Ap.Web.Authentication
{
    class QueryStringTokenExtractor : IBearerTokenExtractor
    {
        public IBearerTokenExtractor Next { get; set; }

        static readonly string SessionId = "session_id";
        private readonly ILogger<QueryStringTokenExtractor> _logger;

        public QueryStringTokenExtractor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QueryStringTokenExtractor>();
        }
        public async ValueTask<string> GetTokenAsync(HttpRequest request)
        {
            _logger.LogInformation("Attempting to get token from QueryString.");
            if (!request.Query.TryGetValue(SessionId, out StringValues session))
            {
                _logger.LogInformation("Attempting to get token from Next.");
                if (null == Next)
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
