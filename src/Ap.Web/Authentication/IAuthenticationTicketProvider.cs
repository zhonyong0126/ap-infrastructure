using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ap.Web.Authentication
{
    public interface IAuthenticationTicketProvider
    {
        ValueTask<AuthenticationTicket> GetTicketAsync(string token, BearerAuthenticationOptions options, HttpContext context);
    }
}
