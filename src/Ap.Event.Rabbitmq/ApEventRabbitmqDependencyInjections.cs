using System;
using System.Collections.Generic;
using Ap.Event.Abstractions;
using Ap.Event.Rabbitmq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApEventRabbitmqDependencyInjections
    {
        public static IServiceCollection AddApEventRabbitmq(this IServiceCollection services)
        {
            services.AddSingleton<IEventPublisher, RabbitMqBasedEventPublisher>();
            services.AddSingleton<IEventSubscriber, RabbitMqbasedEventSubcriber>();
            return services;
        }
    }
}
