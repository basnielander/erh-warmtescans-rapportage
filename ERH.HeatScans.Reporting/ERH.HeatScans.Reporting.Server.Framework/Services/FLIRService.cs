using ERH.FLIR;
using ERH.HeatScans.Reporting.Server.Framework.Models;
using System;
using System.IO;
using System.Threading;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class FLIRService
    {
        private static AppDomain _flirDomain;
        private static readonly object _domainLock = new object();

        private AppDomain GetOrCreateFLIRDomain()
        {
            if (_flirDomain != null)
            {
                return _flirDomain;
            }

            lock (_domainLock)
            {
                if (_flirDomain != null)
                {
                    return _flirDomain;
                }

                // Get the base directory where FLIR assemblies are located
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string flirAssemblyPath = Path.Combine(baseDirectory, "bin");

                // Create AppDomainSetup for FLIR assemblies
                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationBase = baseDirectory,
                    PrivateBinPath = "bin",
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    ShadowCopyFiles = "false",
                    LoaderOptimization = LoaderOptimization.MultiDomainHost
                };

                // Create a new AppDomain for FLIR assemblies
                _flirDomain = AppDomain.CreateDomain(
                    "FLIRDomain",
                    AppDomain.CurrentDomain.Evidence,
                    setup
                );

                return _flirDomain;
            }
        }

        internal FileDownloadResult GetHeatscanImage(Stream imageStream)
        {
            try
            {
                // Get or create the FLIR AppDomain
                AppDomain flirDomain = GetOrCreateFLIRDomain();

                // Create a proxy to execute code in the FLIR AppDomain
                FLIRImageProcessor processor = (FLIRImageProcessor)flirDomain.CreateInstanceAndUnwrap(
                    typeof(FLIRImageProcessor).Assembly.FullName,
                    typeof(FLIRImageProcessor).FullName
                );

                // Process the image in the separate AppDomain
                byte[] imageData = processor.ProcessImage(imageStream);

                return new FileDownloadResult
                {
                    Data = imageData,
                    MimeType = "image/jpeg"
                };
            }
            catch (Exception ex)
            {
                // Log the exception (add logging framework if available)
                System.Diagnostics.Debug.WriteLine($"Error processing heat scan image: {ex.Message}");
                throw;
            }
        }

        public static void UnloadFLIRDomain()
        {
            lock (_domainLock)
            {
                if (_flirDomain != null)
                {
                    try
                    {
                        AppDomain.Unload(_flirDomain);
                    }
                    catch
                    {
                        // Ignore unload errors
                    }
                    finally
                    {
                        _flirDomain = null;
                    }
                }
            }
        }

        internal FileDownloadResult AddSpotToHeatscan(Stream imageStream, int x, int y, CancellationToken cancellationToken)
        {
            try
            {
                // Get or create the FLIR AppDomain
                AppDomain flirDomain = GetOrCreateFLIRDomain();

                // Create a proxy to execute code in the FLIR AppDomain
                FLIRImageProcessor processor = (FLIRImageProcessor)flirDomain.CreateInstanceAndUnwrap(
                    typeof(FLIRImageProcessor).Assembly.FullName,
                    typeof(FLIRImageProcessor).FullName
                );

                // Process the image in the separate AppDomain
                byte[] imageData = processor.ProcessImage(imageStream);

                return new FileDownloadResult
                {
                    Data = imageData,
                    MimeType = "image/jpeg"
                };
            }
            catch (Exception ex)
            {
                // Log the exception (add logging framework if available)
                System.Diagnostics.Debug.WriteLine($"Error processing heat scan image: {ex.Message}");
                throw;
            }
        }
    }

    [Serializable]
    public class FLIRImageProcessor : MarshalByRefObject
    {
        public byte[] ProcessImage(Stream imageStream)
        {
            if (HeatScanImage.IsHeatScanImage(imageStream))
            {
                // This code executes in the separate AppDomain
                return HeatScanImage.ImageInBytes(imageStream, true);
            }

            return [];
        }

        public override object InitializeLifetimeService()
        {
            // Return null to prevent the object from being disposed
            // This makes the remote object have an infinite lifetime
            return null;
        }
    }
}