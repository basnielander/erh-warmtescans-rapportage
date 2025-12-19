using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using ERH.HeatScans.Reporting.Server.Models;
using System.Security.Claims;

namespace ERH.HeatScans.Reporting.Server.Services;

public class UserGoogleDriveService
{
    private readonly ILogger<UserGoogleDriveService> _logger;

    public UserGoogleDriveService(ILogger<UserGoogleDriveService> logger)
    {
        _logger = logger;
    }

    public async Task<GoogleDriveItem> GetFolderStructureAsync(string accessToken, string? folderId = null, CancellationToken cancellationToken = default)
    {
        folderId ??= "1A9-OGvD5LDPzFggsPNG7PF3pKl9xcHvQ";
        
        try
        {
            var driveService = CreateDriveService(accessToken);
            var folder = await GetFileMetadataAsync(driveService, folderId, cancellationToken);
            folder.Children = await GetChildrenAsync(driveService, folderId, cancellationToken);
            
            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder structure for folder ID: {FolderId}", folderId);
            throw;
        }
    }

    public async Task<List<GoogleDriveItem>> GetFlatFileListAsync(string accessToken, string? folderId = null, CancellationToken cancellationToken = default)
    {
        folderId ??= "1A9-OGvD5LDPzFggsPNG7PF3pKl9xcHvQ";
        
        try
        {
            var driveService = CreateDriveService(accessToken);
            var allItems = new List<GoogleDriveItem>();
            await GetAllFilesRecursiveAsync(driveService, folderId, allItems, cancellationToken);
            return allItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flat file list for folder ID: {FolderId}", folderId);
            throw;
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
}
