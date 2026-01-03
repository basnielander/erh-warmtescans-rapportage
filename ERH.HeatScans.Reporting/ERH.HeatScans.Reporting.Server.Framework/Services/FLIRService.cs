using ERH.FLIR;
using ERH.HeatScans.Reporting.Server.Framework.Models;
using System;
using System.IO;
using System.Linq;
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

        private Image ExecuteInFLIRDomain(Func<FLIRImageProcessor, HeatScanImage> processorAction, string errorContext)
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
                var imageData = processorAction(processor);

                return ToImageResult(imageData);
            }
            catch (Exception ex)
            {
                // Log the exception (add logging framework if available)
                System.Diagnostics.Debug.WriteLine($"Error {errorContext}: {ex.Message}");
                throw;
            }
        }

        public Image GetHeatscanImage(Stream imageStream)
        {
            return ExecuteInFLIRDomain(
                processor => processor.ProcessImage(imageStream),
                "processing heat scan image"
            );
        }

        public Image AddSpotToHeatscan(Stream imageStream, double relativeX, double relativeY, CancellationToken cancellationToken)
        {
            return ExecuteInFLIRDomain(
                processor => processor.AddSpot(imageStream, new NewSpot(relativeX, relativeY)),
                "adding a spot to heat scan image"
            );
        }

        public Image CalibrateHeatscan(Stream imageStream, double temperatureMin, double temperatureMax, CancellationToken cancellationToken)
        {
            return ExecuteInFLIRDomain(
                processor => processor.CalibrateImage(imageStream, new TemperatureScale(temperatureMin, temperatureMax)),
                "calibrate (set scale of) heat scan image "
            );
        }

        public Image RemoveSpotFromHeatscan(Stream imageStream, string name, CancellationToken cancellationToken)
        {
            return ExecuteInFLIRDomain(
                processor => processor.RemoveSpot(imageStream, name),
                "removing spot from heat scan image"
            );
        }

        private static Image ToImageResult(HeatScanImage image)
        {
            return new Image
            {
                Data = image.Data,
                MimeType = image.MimeType,
                DateTaken = image.DateTaken,
                Spots = image.Spots.Select(spot => new ImageSpot()
                {
                    Id = spot.Id,
                    Name = spot.Name,
                    Temperature = spot.Temperature,
                    Point = new ImageSpotPoint() { X = spot.X, Y = spot.Y }
                }).ToList(),
                Scale = image.ScaleImage != null ? new ImageScale()
                {
                    Data = image.ScaleImage.Data,
                    MimeType = image.ScaleImage.MimeType,
                    MinTemperature = image.ScaleImage.MinTemperature,
                    MaxTemperature = image.ScaleImage.MaxTemperature
                } : null
            };
        }
    }

    [Serializable]
    public class FLIRImageProcessor : MarshalByRefObject
    {
        public HeatScanImage? ProcessImage(Stream imageStream)
        {
            if (HeatScanImageService.IsHeatScanImage(imageStream))
            {
                // This code executes in the separate AppDomain
                return HeatScanImageService.ImageInBytes(imageStream, true);
            }

            return null;
        }

        public HeatScanImage? AddSpot(Stream imageStream, NewSpot spot)
        {
            if (HeatScanImageService.IsHeatScanImage(imageStream))
            {
                // This code executes in the separate AppDomain
                return HeatScanImageService.AddSpot(imageStream, spot);
            }

            return null;
        }

        public HeatScanImage? CalibrateImage(Stream imageStream, TemperatureScale temperatureScale)
        {
            if (HeatScanImageService.IsHeatScanImage(imageStream))
            {
                // This code executes in the separate AppDomain
                return HeatScanImageService.CalibrateImage(imageStream, temperatureScale);
            }

            return null;
        }

        public HeatScanImage? RemoveSpot(Stream imageStream, string name)
        {
            if (HeatScanImageService.IsHeatScanImage(imageStream))
            {
                // This code executes in the separate AppDomain
                return HeatScanImageService.RemoveSpot(imageStream, name);
            }

            return null;
        }

        public override object InitializeLifetimeService()
        {
            // Return null to prevent the object from being disposed
            // This makes the remote object have an infinite lifetime
            return null;
        }
    }
}