using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ERH.HeatScans.Reporting.Word;

public class WordDocument(MemoryStream frontPagesStream, MemoryStream pageTemplateStream) : IDisposable
{
    private readonly MemoryStream frontPagesStream = frontPagesStream;
    private readonly MemoryStream pageTemplateStream = pageTemplateStream;
    private WordprocessingDocument? wordDocument = null;
    private readonly MemoryStream newDocumentStream = new MemoryStream();
    private bool disposedValue;

    public void Init()
    {
        wordDocument?.Dispose();
        var template = WordprocessingDocument.Open(frontPagesStream, true);

        wordDocument = template.Clone(newDocumentStream, true);
    }

    public void SetReportValue(string property, string value)
    {
        var mainDocumentPart = wordDocument.MainDocumentPart!;
        var body = mainDocumentPart.Document.Body!;

        var propertyRun = body.Elements<Paragraph>().Where(p => p.InnerText.Contains($"{property}:"))
            .FirstOrDefault()
            .Elements<Run>()
            .LastOrDefault();

        if (propertyRun != null)
        {
            propertyRun.AddChild(new Text($"{property}: {value}"));
        }
    }

    public void SetFrontPageImage(Stream frontPageImageStream)
    {
        var mainDocumentPart = wordDocument.MainDocumentPart!;
        var body = mainDocumentPart.Document.Body!;
        var paragraph = body.Elements<Table>().First();

        var irImageCell = GetCell(paragraph, 0, 0);
        ImageHelper.InsertAPicture(wordDocument, "VoordeurFoto", frontPageImageStream, irImageCell, new ImageSize(320, 240), 1.3);
    }

    public void AddPage(PageProperties pageProperties)
    {
        if (wordDocument == null)
        {
            throw new ApplicationException("First initialize a Word document before adding pages");
        }

        var mainDocumentPart = wordDocument.MainDocumentPart!;
        var body = mainDocumentPart.Document.Body!;

        body.AppendChild(new Paragraph(
            new Run(
                new Break() { Type = BreakValues.Page })));

        var template = GetPageTemplateParagraph();

        var irImageCell = GetCell(template, 0, 1);
        ImageHelper.InsertAPicture(wordDocument, $"{pageProperties.ImageName}-warmtescan", pageProperties.HeatScanStream, irImageCell, pageProperties.ImageSize, 1.2);

        var irScaleImageCell = GetCell(template, 0, 2);
        ImageHelper.InsertAPicture(wordDocument, $"{pageProperties.ImageName}-temperatuurschaal", pageProperties.TemperatureScaleStream, irScaleImageCell, new ImageSize(46, 240));

        var visualOnlyCell = GetCell(template, 1, 1);
        ImageHelper.InsertAPicture(wordDocument, $"{pageProperties.ImageName}-foto", pageProperties.DaylightImageStream, visualOnlyCell, pageProperties.ImageSize, 1.2);

        var idCell = GetCell(template, 2, 1);
        var paragraphs = idCell.Elements<Paragraph>().ToList();
        var run = paragraphs.Last().AppendChild(new Run());
        run.AppendChild(new Text(Path.GetFileNameWithoutExtension(pageProperties.ImageName)));

        var newParagraph = body.AppendChild(template);

        if (newParagraph!.Elements<ParagraphProperties>().Count() == 0)
        {
            newParagraph.PrependChild(new ParagraphProperties());
        }

        SetMeasurementsOverview(pageProperties.Measurements, template);
        SetComments(pageProperties.Comments, template);

    }

    private void SetComments(string? comments, Table template)
    {
        var commentsContainerCell = GetCell(template, 0, 0);
        var paragraphs = commentsContainerCell.Elements<Paragraph>().ToList();
        if (string.IsNullOrWhiteSpace(comments))
        {
            paragraphs[2].Remove();
        }
        else
        {
            var run = paragraphs.Last().AppendChild(new Run());
            run.AppendChild(new Text(comments!));
        }
    }

    private static TableCell GetCell(OpenXmlCompositeElement template, int rowIndex, int cellIndex)
    {
        var templateRows = template.Elements<TableRow>().ToList();
        var irRow = templateRows[rowIndex];
        var irCells = irRow.Elements<TableCell>().ToList();
        return irCells[cellIndex];
    }

    private static void SetMeasurementsOverview(Dictionary<string, string> measurements, Table template)
    {
        var measurementsOverviewContainerCell = GetCell(template, 0, 0);
        var measurementsOverviewTable = measurementsOverviewContainerCell.Elements<Table>().ToList().First();

        foreach (var measurement in measurements)
        {
            var index = measurements.ToList().IndexOf(measurement);

            var measurementId = measurement.Key;
            var measurementValue = measurement.Value;

            if (index > 0)
            {
                measurementsOverviewTable.Append(new TableRow(new TableCell(), new TableCell()));
            }

            var measurementIdCell = GetCell(measurementsOverviewTable, index, 0);
            var measurementValueCell = GetCell(measurementsOverviewTable, index, 1);

            measurementIdCell.RemoveAllChildren<Paragraph>();
            measurementValueCell.RemoveAllChildren<Paragraph>();
            SetMeasurementOverviewCell(measurementId, measurementIdCell);
            SetMeasurementOverviewCell(measurementValue, measurementValueCell);
        }
    }

    private static void SetMeasurementOverviewCell(string text, TableCell measurementCell)
    {
        var paragraph = new Paragraph(new Run(new Text()
        {
            Text = text
        }));

        if (paragraph.Elements<ParagraphProperties>().Count() == 0)
        {
            paragraph.PrependChild(new ParagraphProperties());
        }

        // Get the paragraph properties element of the paragraph.
        var props = paragraph.Elements<ParagraphProperties>().First();

        var spacing = new SpacingBetweenLines() { LineRule = LineSpacingRuleValues.Auto, Before = "0", After = "0" };

        props.PrependChild(spacing);

        measurementCell.AppendChild(paragraph);
    }

    public MemoryStream Save()
    {
        if (wordDocument.CanSave)
        {
            wordDocument.Save();
        }

        return newDocumentStream;
    }

    private Table GetPageTemplateParagraph()
    {
        using var document = WordprocessingDocument.Open(pageTemplateStream, true);

        var mainDocumentPart = document.MainDocumentPart!;
        var body = mainDocumentPart.Document.Body!;

        var paragraph = body.Elements<Table>().Last();

        return (paragraph.Clone() as Table)!;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    wordDocument?.Dispose();
                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex.Message);
                    // ignore dispose exceptions
                }

                disposedValue = true;
            }
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}

