using System.Web;
using System.Web.Http;
using ERH.HeatScans.Reporting.Server.Framework.Services;

namespace ERH.HeatScans.Reporting.Server.Framework
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Configure Web API
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_End()
        {
            // Clean up the FLIR AppDomain when the application stops
            FLIRService.UnloadFLIRDomain();
        }
    }
}
