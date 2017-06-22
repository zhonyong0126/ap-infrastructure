using System;

namespace Ap.Web.Authentication
{
    public class JwtBeareAuthenticationOptions
    {
        public Type TicketProviderFactoryType { get; set; }
        public Type TokenExtractorFactoryType { get; set; }
    }
}