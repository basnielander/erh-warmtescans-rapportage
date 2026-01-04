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

        public void CreateDocumentWithoutTheImages(Report report, Stream frontPageImageStream)
        {
            try
            {
                using var frontPage = GetType().Assembly.GetManifestResourceStream("ERH.HeatScans.Reporting.Word.ERH-voorblad.docx");
                using var pageTemplate = GetType().Assembly.GetManifestResourceStream("ERH.HeatScans.Reporting.Word.ERH-pagina.docx");

                wordDocument?.Dispose();

                wordDocument = new WordDocument(frontPage, pageTemplate);
                wordDocument.Init();
                wordDocument.SetReportValue("Object", report.Address);
                wordDocument.SetReportValue("Datum opname", report.PhotosTakenAt.Value.Date.ToShortDateString() ?? "");
                wordDocument.SetReportValue("Temperatuur", $"{report.Temperature}⁰C");
                wordDocument.SetReportValue("Windkracht", $"{report.WindSpeed} km/u");
                wordDocument.SetReportValue("Windrichting", report.WindDirection);
                wordDocument.SetReportValue("Straatzijde", report.FrontDoorDirection);
                wordDocument.SetReportValue("Zonuren", $"{report.HoursOfSunshine} uur");

                wordDocument.SetFrontPageImage(frontPageImageStream);

                wordDocument.Save();
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

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

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
