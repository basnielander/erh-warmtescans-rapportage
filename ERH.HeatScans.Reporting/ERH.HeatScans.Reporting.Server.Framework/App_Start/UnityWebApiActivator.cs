using App_StartERH.HeatScans.Reporting.Server.Framework.Infrastructure;
using System.Web.Http;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ERH.HeatScans.Reporting.Server.Framework.UnityWebApiActivator), nameof(ERH.HeatScans.Reporting.Server.Framework.UnityWebApiActivator.Start))]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(ERH.HeatScans.Reporting.Server.Framework.UnityWebApiActivator), nameof(ERH.HeatScans.Reporting.Server.Framework.UnityWebApiActivator.Shutdown))]

namespace ERH.HeatScans.Reporting.Server.Framework
{
    /// <summary>
    /// Provides the bootstrapping for integrating Unity with WebApi when it is hosted in ASP.NET.
    /// </summary>
    public static class UnityWebApiActivator
    {
        /// <summary>
        /// Integrates Unity when the application starts.
        /// </summary>
        public static void Start()
        {
            // Use UnityHierarchicalDependencyResolver if you want to use
            // a new child container for each IHttpController resolution.
            // var resolver = new UnityHierarchicalDependencyResolver(UnityConfig.Container);
            var resolver = new SafeUnityDependencyResolver(UnityConfig.Container); // new UnityDependencyResolver(UnityConfig.Container);

            GlobalConfiguration.Configuration.DependencyResolver = resolver;
        }

        /// <summary>
        /// Disposes the Unity container when the application is shut down.
        /// </summary>
        public static void Shutdown()
        {
            UnityConfig.Container.Dispose();
        }
    }
}