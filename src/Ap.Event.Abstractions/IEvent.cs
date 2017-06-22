using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Event.Abstractions
{
    public interface IEvent<TEventArgs> where TEventArgs : IEventArgs
    {
        TEventArgs Args { get; }
        string Key { get; }

    }

    public abstract class EventBase<TEventArgs> : IEvent<TEventArgs> where TEventArgs : IEventArgs
    {
        protected EventBase(string key, TEventArgs args)
        {
            Key = key;
            Args = args;
        }

        public string Key { get; private set; }

        public TEventArgs Args { get; private set; }
    }
}
