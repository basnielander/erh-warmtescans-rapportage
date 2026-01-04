using ERH.HeatScans.Reporting.Server.Framework.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class ReportService
    {
        internal async Task<byte[]> CreateReportDocumentAsync(string folderId, Report report, List<Image> heatScans, CancellationToken cancellationToken)
        {
            using var reportDocumentGenerator = new ReportDocumentCreator();

            throw new NotImplementedException();
        }
    }
}