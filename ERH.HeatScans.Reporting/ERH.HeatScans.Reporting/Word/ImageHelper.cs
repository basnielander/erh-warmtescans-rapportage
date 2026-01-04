using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using A = DocumentFormat.OpenXml.Drawing;
using Drawing = DocumentFormat.OpenXml.Wordprocessing.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace ERH.HeatScans.Reporting.Word;

internal static class ImageHelper
{

    internal static void InsertAPicture(WordprocessingDocument wordprocessingDocument, string imageName, Stream imageStream, TableCell tableCell, ImageSize imageSize, double zoomFactor = 1)
    {

        if (wordprocessingDocument.MainDocumentPart is null)
        {
            throw new ArgumentNullException("MainDocumentPart is null.");
        }

        MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

        ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

        imagePart.FeedData(imageStream);

        AddImageToBody(mainPart.GetIdOfPart(imagePart), imageName, imageStream, tableCell, imageSize, zoomFactor);
    }

    static void AddImageToBody(string relationshipId, string imageName, Stream imageStream, TableCell tableCell, ImageSize imageSize, double zoomFactor = 1)
    {
        int imageWidthInCell = (int)(imageSize.Width * 11000 * zoomFactor);
        int imageHeightInCell = (int)(imageSize.Height * 11000 * zoomFactor);

        // Define the reference of the image.
        var element =
             new Drawing(
                 new DW.Inline(
                     new DW.Extent() { Cx = imageWidthInCell, Cy = imageHeightInCell },
                     new DW.EffectExtent()
                     {
                         LeftEdge = 0L,
                         TopEdge = 0L,
                         RightEdge = 0L,
                         BottomEdge = 0L
                     },
                     new DW.DocProperties()
                     {
                         Id = (UInt32Value)1U,
                         Name = Path.GetFileNameWithoutExtension(imageName)
                     },
                     new DW.NonVisualGraphicFrameDrawingProperties(
                         new A.GraphicFrameLocks() { NoChangeAspect = true }),
                     new A.Graphic(
                         new A.GraphicData(
                             new PIC.Picture(
                                 new PIC.NonVisualPictureProperties(
                                     new PIC.NonVisualDrawingProperties()
                                     {
                                         Id = (UInt32Value)0U,
                                         Name = imageName
                                     },
                                     new PIC.NonVisualPictureDrawingProperties()),
                                 new PIC.BlipFill(
                                     new A.Blip(
                                         new A.BlipExtensionList(
                                             new A.BlipExtension()
                                             {
                                                 Uri =
                                                    "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                             })
                                     )
                                     {
                                         Embed = relationshipId,
                                         CompressionState =
                                         A.BlipCompressionValues.Print
                                     },
                                     new A.Stretch(
                                         new A.FillRectangle())),
                                 new PIC.ShapeProperties(
                                     new A.Transform2D(
                                         new A.Offset() { X = 0L, Y = 0L },
                                         new A.Extents() { Cx = imageWidthInCell, Cy = imageHeightInCell }),
                                     new A.PresetGeometry(
                                         new A.AdjustValueList()
                                     )
                                     { Preset = A.ShapeTypeValues.Rectangle }))
                         )
                         { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                 )
                 {
                     DistanceFromTop = (UInt32Value)0U,
                     DistanceFromBottom = (UInt32Value)0U,
                     DistanceFromLeft = (UInt32Value)0U,
                     DistanceFromRight = (UInt32Value)0U,
                     EditId = "50D07946"
                 });

        // Append the reference to body, the element should be in a Run.
        tableCell.Append(new Paragraph(new Run(element)));

    }

    static ImageSize GetImageSize(FileInfo imageFile)
    {
        using var image = System.Drawing.Image.FromFile(imageFile.FullName);

        return new(image.Width, image.Height);
    }

}

public class ImageSize(int Width, int Height)
{
    public int Width { get; } = Width;
    public int Height { get; } = Height;
}
