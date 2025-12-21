using System;
using System.Web;
using System.Web.Http;
using Unity;
using Unity.AspNet.WebApi;
using ERH.HeatScans.Reporting.Server.Framework.Services;

namespace ERH.HeatScans.Reporting.Server.Framework
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Configure Web API
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Configure Dependency Injection
            var container = new UnityContainer();
            ConfigureDependencyInjection(container);
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }

        private void ConfigureDependencyInjection(UnityContainer container)
        {
            // Register services
            container.RegisterType<UserGoogleDriveService>();
        }
               
    }
}
