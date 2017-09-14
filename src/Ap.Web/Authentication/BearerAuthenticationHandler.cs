using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

namespace Ap.Web.Authentication
{
    public class BearerAuthenticationHandler : AuthenticationHandler<BearerAuthenticationOptions>
    {
        private readonly IBearerTokenExtractor _tokenExtractor;
        private readonly IAuthenticationTicketProvider _ticketProvider;

        //IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock
        public BearerAuthenticationHandler(IBearerTokenExtractor tokenExractor, IAuthenticationTicketProvider ticketProvider
            ,IOptionsMonitor<BearerAuthenticationOptions> options
            ,ILoggerFactory logger
            ,UrlEncoder encoder
            ,ISystemClock clock)
            :base(options,logger,encoder,clock)
        {
            _tokenExtractor = tokenExractor;
            _ticketProvider = ticketProvider;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var token = await _tokenExtractor.GetTokenAsync(Context.Request).ConfigureAwait(false);
                if (string.IsNullOrEmpty(token))
                {
                    return AuthenticateResult.NoResult();
                }
                var ticket = await _ticketProvider.GetTicketAsync(token, Options, Context);
                if (ticket != null)
                {
                    return AuthenticateResult.Success(ticket);
                }
                return AuthenticateResult.Fail("Null Ticket");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }


        // protected override Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        // {
        //     return base.HandleUnauthorizedAsync(context);
        // }
    }
}
