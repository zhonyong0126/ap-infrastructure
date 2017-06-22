using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ap.Event.Abstractions
{
    public interface IEventSubscriber
    {
        Task<Guid> SubscribeAsync<TEventArgs>(string queue, IEvent<TEventArgs> @event, Func<TEventArgs, Task> callback) where TEventArgs : IEventArgs;
        void Unsubscribe(Guid token);
    }
}
