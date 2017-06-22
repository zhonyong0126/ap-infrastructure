using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ap.Infrastructure
{
    class DomainEventEmitter : IDomainEventEmitter
    {
        public DomainEventEmitter(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public async Task EmitAsync<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            var services = _serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();
            var handlerResultTasks = new List<Task>();
            foreach (var srv in services)
            {
                var resultTask = Task.Run(() => srv.HandleAsync(domainEvent));
                handlerResultTasks.Add(resultTask);
            }
            await Task.WhenAll(handlerResultTasks);
        }

        private readonly IServiceProvider _serviceProvider;
    }
}
