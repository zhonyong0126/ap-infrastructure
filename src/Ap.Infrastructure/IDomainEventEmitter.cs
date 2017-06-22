using System.Threading.Tasks;

namespace Ap.Infrastructure
{
    public interface IDomainEventEmitter
    {
        Task EmitAsync<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent;
    }
}
