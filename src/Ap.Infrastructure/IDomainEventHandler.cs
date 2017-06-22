using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ap.Infrastructure
{
    public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
        Task HandleAsync(TDomainEvent domainEvent);
    }
}
