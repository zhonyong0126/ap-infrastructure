using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Ap.Web.Authentication
{
    class BearerAuthenticationMiddleware : AuthenticationMiddleware<BearerAuthenticationOptions>
    {
        private readonly IBearerTokenExtractor _tokenExtractor;
        private readonly IAuthenticationTicketProvider _ticketProvider;

        public BearerAuthenticationMiddleware(IBearerTokenExtractorFactory tokenExtractorFactory, IAuthenticationTicketProviderFactory ticketProviderBuilder, RequestDelegate next, IOptions<BearerAuthenticationOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder) : base(next, options, loggerFactory, encoder)
        {
            AuthenticationScheme = options.Value.AuthenticationScheme;
            _tokenExtractor = tokenExtractorFactory.Create();
            _ticketProvider = ticketProviderBuilder.Build();
        }

        protected override AuthenticationHandler<BearerAuthenticationOptions> CreateHandler()
        {
            return new BearerAuthenticationHandler(_tokenExtractor, _ticketProvider);
        }
    }
}
