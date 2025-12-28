using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
using System.IO;
using System.Web;
using System.Web.Http;
using Unity;

namespace ERH.HeatScans.Reporting.Server.Framework
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Configure Web API
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Ensure FLIR native dependencies can be resolved from bin directories
            ConfigureFlirAssemblyResolution();

            // Configure Dependency Injection
            var container = new UnityContainer();
            ConfigureDependencyInjection(container);
            // GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }

        private void ConfigureDependencyInjection(UnityContainer container)
        {
            // Register services
            container.RegisterType<GoogleDriveService>();
            container.RegisterType<FLIRService>();
        }

        private static void ConfigureFlirAssemblyResolution()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var binDir = Path.Combine(baseDir, "bin");
            var x64Dir = Path.Combine(binDir, "x64");

            // Prepend bin paths to the probing list by setting PrivateBinPath
            AppDomain.CurrentDomain.AppendPrivatePath(@"C:\Projects\Nuget\packages\Flir.Atlas.Cronos.7.6.0\build\net452\win-x64\lib");
            // AppDomain.CurrentDomain.AppendPrivatePath(Path.Combine("bin", "x64"));

            // Assembly resolve fallback for FLIR native/managed assemblies
            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    try
            //    {
            //        var name = new AssemblyName(args.Name).Name + ".dll";

            //        // Try bin
            //        var candidate = Path.Combine(binDir, name);
            //        if (File.Exists(candidate))
            //        {
            //            return Assembly.LoadFrom(candidate);
            //        }

            //        // Try bin\x64
            //        candidate = Path.Combine(x64Dir, name);
            //        if (File.Exists(candidate))
            //        {
            //            return Assembly.LoadFrom(candidate);
            //        }
            //    }
            //    catch
            //    {
            //        // Swallow and let default resolution continue
            //    }

            //    return null;
            //};
        }
    }
}
