using System.IO;

namespace ERH.HeatScans.Reporting;

public class ReportImage
{
    public ReportImage(string id, int index)
    {
        this.Id = id;
        this.Index = index;
    }

    public string Id { get; private set; }
    public string MimeType { get; set; }
    public long? Size { get; set; }
    public string ModifiedTime { get; set; }

    public string Name { get; set; }

    //public MeasurementCollection Measurements
    //{
    //    get
    //    {
    //        using var thermalFile = new ThermalImageFile(this.File.FullName);
    //        return thermalFile.Measurements;
    //    }
    //}

    public string? Comments { get; set; }

    public bool ExcludeFromReport { get; set; } = false;


    /// <summary>
    /// Temperatuurschaal (min/max)
    /// </summary>
    //public Range<double> Scale
    //{
    //    get
    //    {
    //        using var thermalFile = new ThermalImageFile(this.File.FullName);
    //        return thermalFile.Scale.Range;
    //    }
    //}

    public string? TemperatureScaleImageFile
    {
        get; set;
    }

    public int Index { get; set; }



    private string VisualOnlyName
    {
        get
        {
            var targetFileExtension = Path.GetExtension(Name);
            var targetFileWithoutExtension = Path.GetFileNameWithoutExtension(Name);

            return $"{targetFileWithoutExtension}-visual{targetFileExtension}";
        }
    }

    //internal ReportImageVariants CopyTo(DirectoryInfo targetDirectory, bool createVisualImage)
    //{
    //    if (createVisualImage)
    //    {
    //        var thermalImageInBytes = System.IO.File.ReadAllBytes(File.FullName);
    //        var targetIrFile = new FileInfo(Path.Combine(targetDirectory.FullName, Name));

    //        var targetVisualOnlyFile = Path.Combine(targetDirectory.FullName, VisualOnlyName);
    //        var visualOnlyFile = new FileInfo(targetVisualOnlyFile);

    //        var imageSize = SaveImageWithMeasurements(thermalImageInBytes, targetIrFile);
    //        SaveDaylightImage(visualOnlyFile);

    //        return new ReportImageVariants(targetIrFile, visualOnlyFile, imageSize);
    //    }

    //    return new ReportImageVariants(null, null, null);
    //}
}

//public class ReportImageVariants(FileInfo IrImageFile, FileInfo VisualOnlyImageFile, ImageSize ImageSize)
//{
//    public FileInfo IrImageFile { get; } = IrImageFile;
//    public FileInfo? VisualOnlyImageFile { get; } = VisualOnlyImageFile;
//    public ImageSize ImageSize { get; } = ImageSize;
//};
