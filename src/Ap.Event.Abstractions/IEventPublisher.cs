using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ap.Event.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEventArgs>(IEvent<TEventArgs> @event) where TEventArgs : IEventArgs;
    }
}
