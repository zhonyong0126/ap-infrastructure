using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading;

namespace Ap.Web.Authentication
{
    public class MemoryCachedTicketProvider : IAuthenticationTicketProvider
    {
        private readonly IAuthenticationTicketProvider _next;
        private readonly ConcurrentDictionary<string, AuthenticationTicket> _cache = new ConcurrentDictionary<string, AuthenticationTicket>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockers = new ConcurrentDictionary<string, SemaphoreSlim>();

        public MemoryCachedTicketProvider(IAuthenticationTicketProvider next)
        {
            _next = next;
        }

        public async ValueTask<AuthenticationTicket> GetTicketAsync(string token, BearerAuthenticationOptions options, HttpContext context)
        {
            var existing = _cache.TryGetValue(token, out AuthenticationTicket ticket);
            if (existing)
            {
                return ticket;
            }

            if (null==_next)
            {
                return null;
            }

            var locker = _lockers.GetOrAdd(token, t => new SemaphoreSlim(1, 1));
            await locker.WaitAsync();
            try
            {
                ticket = await _next.GetTicketAsync(token,options,context);
                if (null!=ticket)
                {
                    _cache.TryAdd(token, ticket);
                }
                return ticket;
            }
            finally
            {
                locker.Release();
            }
        }
    }
}
