using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ap.Infrastructure
{
    public interface ICacheExpirationNotifier
    {
        IDisposable RegisterExpirationCallback(string cacheTicket, Func<Task> callback);
    }
}
