using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Event.Rabbitmq
{
    interface ISubscriber
    {
        string QueueName { get; }
        string RoutingKey { get; }
        string ConsumerTag { get; }
        Delegate Callback { get; }
        IDictionary<string, object> Arguments { get; }
        RuntimeTypeHandle EventArgsTypeHandle { get; }
    }
}
