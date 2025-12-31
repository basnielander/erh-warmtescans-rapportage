using ERH.HeatScans.Reporting.Server.Framework.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class GoogleDriveService
    {
        public async Task<GoogleDriveItem> GetFolderStructureAsync(string accessToken, string folderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var driveService = CreateDriveService(accessToken);
                var folder = await GetFileMetadataAsync(driveService, folderId, cancellationToken);
                folder.Children = await GetChildrenAsync(driveService, folderId, cancellationToken);

                return folder;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving folder structure for folder ID: {folderId}", ex);
            }
        }

        public async Task<List<GoogleDriveItem>> GetFlatFileListAsync(string accessToken, string folderId = null, CancellationToken cancellationToken = default)
        {
            folderId = folderId ?? "1A9-OGvD5LDPzFggsPNG7PF3pKl9xcHvQ";

            try
            {
                var driveService = CreateDriveService(accessToken);
                var allItems = new List<GoogleDriveItem>();
                await GetAllFilesRecursiveAsync(driveService, folderId, allItems, cancellationToken);
                return allItems;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving flat file list for folder ID: {folderId}", ex);
            }
        }

        private DriveService CreateDriveService(string accessToken)
        {
            var credential = GoogleCredential.FromAccessToken(accessToken);

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ERH Heat Scans Reporting"
            });
        }

        private async Task<GoogleDriveItem> GetFileMetadataAsync(DriveService driveService, string fileId, CancellationToken cancellationToken)
        {
            var request = driveService.Files.Get(fileId);
            request.Fields = "id, name, mimeType, modifiedTime, size";

            var file = await request.ExecuteAsync(cancellationToken);

            return new GoogleDriveItem
            {
                Id = file.Id,
                Name = file.Name,
                MimeType = file.MimeType,
                IsFolder = file.MimeType == "application/vnd.google-apps.folder",
                ModifiedTime = file.ModifiedTimeDateTimeOffset?.DateTime,
                Size = file.Size
            };
        }

        private async Task<List<GoogleDriveItem>> GetChildrenAsync(DriveService driveService, string folderId, CancellationToken cancellationToken)
        {
            var children = new List<GoogleDriveItem>();

            var request = driveService.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Fields = "nextPageToken, files(id, name, mimeType, modifiedTime, size)";
            request.PageSize = 500;

            do
            {
                var result = await request.ExecuteAsync(cancellationToken);

                if (result.Files != null)
                {
                    foreach (var file in result.Files)
                    {
                        var item = new GoogleDriveItem
                        {
                            Id = file.Id,
                            Name = file.Name,
                            MimeType = file.MimeType,
                            IsFolder = file.MimeType == "application/vnd.google-apps.folder",
                            ModifiedTime = file.ModifiedTimeDateTimeOffset?.DateTime,
                            Size = file.Size
                        };

                        if (item.IsFolder)
                        {
                            item.Children = await GetChildrenAsync(driveService, file.Id, cancellationToken);
                        }

                        children.Add(item);
                    }
                }

                request.PageToken = result.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));

            return children;
        }

        private async Task GetAllFilesRecursiveAsync(DriveService driveService, string folderId, List<GoogleDriveItem> allItems, CancellationToken cancellationToken)
        {
            var request = driveService.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Fields = "nextPageToken, files(id, name, mimeType, modifiedTime, size)";
            request.PageSize = 1000;

            do
            {
                var result = await request.ExecuteAsync(cancellationToken);

                if (result.Files != null)
                {
                    foreach (var file in result.Files)
                    {
                        var item = new GoogleDriveItem
                        {
                            Id = file.Id,
                            Name = file.Name,
                            MimeType = file.MimeType,
                            IsFolder = file.MimeType == "application/vnd.google-apps.folder",
                            ModifiedTime = file.ModifiedTimeDateTimeOffset?.DateTime,
                            Size = file.Size
                        };

                        allItems.Add(item);

                        if (item.IsFolder)
                        {
                            await GetAllFilesRecursiveAsync(driveService, file.Id, allItems, cancellationToken);
                        }
                    }
                }

                request.PageToken = result.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));
        }

        internal async Task SetupAddressFolderAsync(string accessToken, string addressFolderId, CancellationToken cancellationToken)
        {
            try
            {
                var driveService = CreateDriveService(accessToken);

                // Get all items in the address folder
                var addressFolderItems = await GetChildrenAsync(driveService, addressFolderId, cancellationToken);

                // Check if "1. Origineel" subfolder exists
                var origineelFolder = addressFolderItems.FirstOrDefault(item =>
                    item.IsFolder && item.Name == "1. Origineel");

                string origineelFolderId;
                if (origineelFolder == null)
                {
                    // Create "1. Origineel" subfolder
                    origineelFolderId = await CreateFolderAsync(driveService, "1. Origineel", addressFolderId, cancellationToken);
                }
                else
                {
                    origineelFolderId = origineelFolder.Id;
                }

                // Check if "2. Bewerkt" subfolder exists
                var bewerktFolder = addressFolderItems.FirstOrDefault(item =>
                    item.IsFolder && item.Name == "2. Bewerkt");

                string bewerktFolderId;
                if (bewerktFolder == null)
                {
                    // Create "2. Bewerkt" subfolder
                    bewerktFolderId = await CreateFolderAsync(driveService, "2. Bewerkt", addressFolderId, cancellationToken);
                }
                else
                {
                    bewerktFolderId = bewerktFolder.Id;
                }

                // Find image files in the address folder (not in subfolders)
                var imageExtensions = new[] { ".jpg", ".jpeg" };
                var imageFiles = addressFolderItems.Where(item =>
                    !item.IsFolder &&
                    imageExtensions.Any(ext => item.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (imageFiles.Any())
                {
                    // Move image files to "1. Origineel" subfolder
                    foreach (var imageFile in imageFiles)
                    {
                        await MoveFileAsync(driveService, imageFile.Id, addressFolderId, origineelFolderId, cancellationToken);
                    }

                    // Refresh the folder items after moving files
                    addressFolderItems = await GetChildrenAsync(driveService, addressFolderId, cancellationToken);

                    // Get the files from "1. Origineel" folder
                    var origineelFolderItems = await GetChildrenAsync(driveService, origineelFolderId, cancellationToken);

                    // Copy all files from "1. Origineel" to "2. Bewerkt"
                    foreach (var file in origineelFolderItems.Where(item => !item.IsFolder))
                    {
                        await CopyFileAsync(driveService, file.Id, bewerktFolderId, file.Name, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting up address folder with ID: {addressFolderId}", ex);
            }
        }

        private async Task<string> CreateFolderAsync(DriveService driveService, string folderName, string parentFolderId, CancellationToken cancellationToken)
        {
            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentFolderId }
            };

            var request = driveService.Files.Create(folderMetadata);
            request.Fields = "id";

            var folder = await request.ExecuteAsync(cancellationToken);
            return folder.Id;
        }

        private async Task MoveFileAsync(DriveService driveService, string fileId, string oldParentId, string newParentId, CancellationToken cancellationToken)
        {
            var request = driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), fileId);
            request.AddParents = newParentId;
            request.RemoveParents = oldParentId;
            request.Fields = "id, parents";

            await request.ExecuteAsync(cancellationToken);
        }

        private async Task<string> CopyFileAsync(DriveService driveService, string fileId, string destinationFolderId, string fileName, CancellationToken cancellationToken)
        {
            var copyMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { destinationFolderId }
            };

            var request = driveService.Files.Copy(copyMetadata, fileId);
            request.Fields = "id";

            var copiedFile = await request.ExecuteAsync(cancellationToken);
            return copiedFile.Id;
        }

        public async Task<Report> GetReportAsync(string accessToken, string addressFolderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var driveService = CreateDriveService(accessToken);

                var addressFolder = await GetFileMetadataAsync(driveService, addressFolderId, cancellationToken);

                // Get all items in the address folder
                var addressFolderItems = await GetChildrenAsync(driveService, addressFolderId, cancellationToken);

                // Find "2. Bewerkt" subfolder
                var bewerktFolder = addressFolderItems.FirstOrDefault(item =>
                    item.IsFolder && item.Name == "2. Bewerkt");

                if (bewerktFolder == null)
                {
                    throw new Exception($"Subfolder '2. Bewerkt' not found in folder {addressFolderId}");
                }

                string bewerktFolderId = bewerktFolder.Id;

                // Get all items in "2. Bewerkt" folder
                var bewerktFolderItems = await GetChildrenAsync(driveService, bewerktFolderId, cancellationToken);

                // Look for report.json file
                var reportFile = bewerktFolderItems.FirstOrDefault(item =>
                    !item.IsFolder && item.Name.Equals("report.json", StringComparison.OrdinalIgnoreCase));

                Report report;

                if (reportFile != null)
                {
                    // Read existing report.json
                    report = await ReadReportFileAsync(driveService, reportFile.Id, cancellationToken);
                }
                else
                {
                    // Create new report
                    report = new Report(addressFolder.Id, addressFolder.Name);
                }

                // Get all image files from "2. Bewerkt" folder
                var imageExtensions = new[] { ".jpg", ".jpeg" };
                var imageFiles = bewerktFolderItems.Where(item =>
                    !item.IsFolder &&
                    imageExtensions.Any(ext => item.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                // Update report with current images
                report.Images = [];

                var index = 0;
                foreach (var heatScan in imageFiles)
                {
                    report.Images.Add(new ReportImage(heatScan.Id, index)
                    {
                        Name = heatScan.Name,
                        MimeType = heatScan.MimeType,
                        Size = heatScan.Size,
                        ModifiedTime = heatScan.ModifiedTime?.ToString("o"), // ISO 8601 format
                    });

                    index++;
                }

                // Save or update report.json
                if (reportFile != null)
                {
                    await UpdateReportFileAsync(driveService, reportFile.Id, report, cancellationToken);
                }
                else
                {
                    await CreateReportFileAsync(driveService, bewerktFolderId, report, cancellationToken);
                }

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting report for folder ID: {addressFolderId}", ex);
            }
        }


        internal async Task<Stream> GetFileBytesAsync(string accessToken, string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var driveService = CreateDriveService(accessToken);

                var metadataRequest = driveService.Files.Get(fileId);
                metadataRequest.Fields = "id, name, mimeType";
                var fileMetadata = await metadataRequest.ExecuteAsync(cancellationToken);

                var request = driveService.Files.Get(fileId);
                var stream = new MemoryStream();
                await request.DownloadAsync(stream, cancellationToken);

                return stream;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file with ID: {fileId}", ex);
            }
        }
        private async Task<Report> ReadReportFileAsync(DriveService driveService, string fileId, CancellationToken cancellationToken)
        {
            var request = driveService.Files.Get(fileId);
            var stream = new MemoryStream();
            await request.DownloadAsync(stream, cancellationToken);

            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Report>(json);
            }
        }

        private async Task<string> CreateReportFileAsync(DriveService driveService, string parentFolderId, Report report, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(report, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "report.json",
                MimeType = "application/json",
                Parents = new List<string> { parentFolderId }
            };

            using (var stream = new MemoryStream(bytes))
            {
                var request = driveService.Files.Create(fileMetadata, stream, "application/json");
                request.Fields = "id";
                var file = await request.UploadAsync(cancellationToken);

                if (file.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new Exception("Failed to upload report.json file");
                }

                return request.ResponseBody.Id;
            }
        }

        private async Task UpdateReportFileAsync(DriveService driveService, string fileId, Report report, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(report, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File();

            using (var stream = new MemoryStream(bytes))
            {
                var request = driveService.Files.Update(fileMetadata, fileId, stream, "application/json");
                var file = await request.UploadAsync(cancellationToken);

                if (file.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new Exception("Failed to update report.json file");
                }
            }
        }
    }
}
