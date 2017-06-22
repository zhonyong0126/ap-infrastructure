
using Ap.Infrastructure;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApInfrastructureDependencyInjection
    {
        public static IServiceCollection AddApInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDomainEventEmitter, DomainEventEmitter>();
            return services;
        }
    }
}
