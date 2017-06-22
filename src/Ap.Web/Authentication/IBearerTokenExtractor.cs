using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ap.Web.Authentication
{
    public interface IBearerTokenExtractor
    {
        ValueTask<string> GetTokenAsync(HttpRequest request);
        IBearerTokenExtractor Next { get; set; }
    }
}
