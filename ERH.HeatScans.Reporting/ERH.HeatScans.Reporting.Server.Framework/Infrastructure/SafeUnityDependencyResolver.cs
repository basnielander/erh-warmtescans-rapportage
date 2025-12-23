using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Unity;

namespace App_StartERH.HeatScans.Reporting.Server.Framework.Infrastructure
{
    /// <summary>
    /// A safe wrapper for Unity dependency resolver that properly handles all resolution exceptions
    /// for optional ASP.NET Web API services.
    /// </summary>
    public class SafeUnityDependencyResolver : IDependencyResolver
    {
        private readonly IUnityContainer _container;
        private readonly SharedDependencyScope _sharedScope;

        public SafeUnityDependencyResolver(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _sharedScope = new SharedDependencyScope(container);
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (Exception ex) when (ex is ResolutionFailedException ||
                                        ex is InvalidOperationException)
            {
                // Optional services should return null when not registered
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (Exception ex) when (ex is ResolutionFailedException ||
                                        ex is InvalidOperationException)
            {
                // Return empty collection for unregistered services
                return new object[0];
            }
        }

        public IDependencyScope BeginScope()
        {
            return _sharedScope;
        }

        public void Dispose()
        {
            _container.Dispose();
            _sharedScope?.Dispose();
        }

        private sealed class SharedDependencyScope : IDependencyScope
        {
            private readonly IUnityContainer _container;

            public SharedDependencyScope(IUnityContainer container)
            {
                _container = container;
            }

            public object GetService(Type serviceType)
            {
                try
                {
                    return _container.Resolve(serviceType);
                }
                catch (Exception ex) when (ex is ResolutionFailedException ||
                                            ex is InvalidOperationException)
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                try
                {
                    return _container.ResolveAll(serviceType);
                }
                catch (Exception ex) when (ex is ResolutionFailedException ||
                                            ex is InvalidOperationException)
                {
                    return new object[0];
                }
            }

            public void Dispose()
            {
                // Shared scope doesn't dispose the container
            }
        }
    }
}