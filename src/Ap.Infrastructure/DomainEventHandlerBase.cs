using System.Threading.Tasks;

namespace Ap.Infrastructure
{
    public abstract class DomainEventHandlerBase<TDomainEvent> : IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
        public Task HandleAsync(TDomainEvent domainEvent)
        {
            return InternalHandleAsync(domainEvent);
        }

        protected abstract Task InternalHandleAsync(TDomainEvent domainEvent);
    }
}
