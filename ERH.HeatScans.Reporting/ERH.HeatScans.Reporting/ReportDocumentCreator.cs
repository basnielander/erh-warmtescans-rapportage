using ERH.HeatScans.Reporting.Word;
using System;
using System.Diagnostics;
using System.IO;

namespace ERH.HeatScans.Reporting
{
    public sealed class ReportDocumentCreator : IDisposable
    {
        private bool disposed;

        private WordDocument? wordDocument = null;
        private readonly MemoryStream frontPageWritable = new();
        private readonly MemoryStream pageTemplateWritable = new();

        public void CreateDocumentWithoutTheImages(Report report, Stream frontPageImageStream)
        {
            try
            {
                using var frontPage = GetType().Assembly.GetManifestResourceStream("ERH.HeatScans.Reporting.Word.ERH-voorblad.docx");
                using var pageTemplate = GetType().Assembly.GetManifestResourceStream("ERH.HeatScans.Reporting.Word.ERH-pagina.docx");

                // create a writable memory stream from frontpage and pagetemplate
                frontPage?.CopyTo(frontPageWritable);
                frontPageWritable.Position = 0;

                pageTemplate?.CopyTo(pageTemplateWritable);
                pageTemplateWritable.Position = 0;

                wordDocument?.Dispose();

                wordDocument = new WordDocument(frontPageWritable, pageTemplateWritable);
                wordDocument.Init();
                wordDocument.SetReportValue("Object", report.Address);
                wordDocument.SetReportValue("Datum opname", report.PhotosTakenAt);
                wordDocument.SetReportValue("Temperatuur", $"{report.Temperature}⁰C");
                wordDocument.SetReportValue("Windkracht", $"{report.WindSpeed} km/u");
                wordDocument.SetReportValue("Windrichting", report.WindDirection);
                wordDocument.SetReportValue("Straatzijde", report.FrontDoorDirection);
                wordDocument.SetReportValue("Zonuren", $"{report.HoursOfSunshine} uur");

                wordDocument.SetFrontPageImage(frontPageImageStream);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
                throw;
            }
        }

        public void AddPage(PageProperties page)
        {
            ThrowIfDisposed();
            if (wordDocument == null)
            {
                throw new ApplicationException("First Create Document Without The Images, to initiate the Word document, before adding pages");
            }

            wordDocument.AddPage(page);
        }

        public MemoryStream Save()
        {
            ThrowIfDisposed();
            if (wordDocument == null)
            {
                throw new ApplicationException("First Create Document Without The Images, to initiate the Word document, before adding pages, after which Save can be called");
            }

            return wordDocument.Save();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            frontPageWritable.Dispose();
            pageTemplateWritable.Dispose();
            wordDocument.Dispose();

            disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

    }
}
