using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using ERH.HeatScans.Reporting.Server.Models;

namespace ERH.HeatScans.Reporting.Server.Services;

public class GoogleDriveService
{
    private readonly DriveService _driveService;
    private readonly ILogger<GoogleDriveService> _logger;

    public GoogleDriveService(IConfiguration configuration, ILogger<GoogleDriveService> logger)
    {
        _logger = logger;
        
        var credentialPath = configuration["GoogleDrive:CredentialPath"];
        
        if (string.IsNullOrEmpty(credentialPath) || !File.Exists(credentialPath))
        {
            throw new InvalidOperationException(
                "Google Drive credential file not found. Please configure GoogleDrive:CredentialPath in appsettings.json");
        }

        GoogleCredential credential;
        using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStreamAsync(stream, CancellationToken.None).GetAwaiter().GetResult();
            credential = credential.CreateScoped(DriveService.ScopeConstants.DriveReadonly);
        }

        _driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "ERH Heat Scans Reporting"
        });
    }

    public async Task<GoogleDriveItem> GetFolderStructureAsync(string? folderId = null, CancellationToken cancellationToken = default)
    {
        folderId ??= "1A9-OGvD5LDPzFggsPNG7PF3pKl9xcHvQ";
        
        try
        {
            var folder = await GetFileMetadataAsync(folderId, cancellationToken);
            folder.Children = await GetChildrenAsync(folderId, cancellationToken);
            
            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder structure for folder ID: {FolderId}", folderId);
            throw;
        }
    }

    public async Task<List<GoogleDriveItem>> GetFlatFileListAsync(string? folderId = null, CancellationToken cancellationToken = default)
    {
        folderId ??= "1A9-OGvD5LDPzFggsPNG7PF3pKl9xcHvQ";
        
        try
        {
            var allItems = new List<GoogleDriveItem>();
            await GetAllFilesRecursiveAsync(folderId, allItems, cancellationToken);
            return allItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flat file list for folder ID: {FolderId}", folderId);
            throw;
        }
    }

    private async Task<GoogleDriveItem> GetFileMetadataAsync(string fileId, CancellationToken cancellationToken)
    {
        var request = _driveService.Files.Get(fileId);
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

    private async Task<List<GoogleDriveItem>> GetChildrenAsync(string folderId, CancellationToken cancellationToken)
    {
        var children = new List<GoogleDriveItem>();
        
        var request = _driveService.Files.List();
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
                        item.Children = await GetChildrenAsync(file.Id, cancellationToken);
                    }

                    children.Add(item);
                }
            }

            request.PageToken = result.NextPageToken;
        } while (!string.IsNullOrEmpty(request.PageToken));

        return children;
    }

    private async Task GetAllFilesRecursiveAsync(string folderId, List<GoogleDriveItem> allItems, CancellationToken cancellationToken)
    {
        var request = _driveService.Files.List();
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
                        await GetAllFilesRecursiveAsync(file.Id, allItems, cancellationToken);
                    }
                }
            }

            request.PageToken = result.NextPageToken;
        } while (!string.IsNullOrEmpty(request.PageToken));
    }
}
