using System.Collections.Generic;
using System;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GhUsersTat
{
    public class ServiceCollectionDependencyResolver : IDependencyResolver
    {
        protected IServiceProvider ServiceProvider;

        public ServiceCollectionDependencyResolver(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
