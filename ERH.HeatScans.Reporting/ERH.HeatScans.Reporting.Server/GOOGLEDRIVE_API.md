# Google Drive API Integration

## Setup

### 1. Google Cloud Console Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google Drive API
4. Create credentials:
   - Go to "Credentials" ? "Create Credentials" ? "Service Account"
   - Fill in the service account details
   - Grant necessary roles (e.g., "Viewer" for read-only access)
   - Create and download the JSON key file

### 2. Share Google Drive Folder

Share the Google Drive folder you want to access with the service account email address (found in the credentials JSON file).

### 3. Configuration

Update `appsettings.json` with the path to your credentials file:

```json
{
  "GoogleDrive": {
    "CredentialPath": "path/to/your/google-credentials.json"
  }
}
```

For production, use `appsettings.Production.json` or environment variables:
```bash
GoogleDrive__CredentialPath=/secure/path/to/credentials.json
```

## API Endpoints

### Get Folder Structure (Hierarchical)

```
GET /api/googledrive/structure?folderId={folderId}
```

**Parameters:**
- `folderId` (optional): The Google Drive folder ID. If not provided, uses the root folder.

**Response:**
```json
{
  "id": "folder-id",
  "name": "Folder Name",
  "mimeType": "application/vnd.google-apps.folder",
  "isFolder": true,
  "modifiedTime": "2024-01-15T10:30:00Z",
  "size": null,
  "children": [
    {
      "id": "file-id",
      "name": "document.pdf",
      "mimeType": "application/pdf",
      "isFolder": false,
      "modifiedTime": "2024-01-15T10:30:00Z",
      "size": 1024000,
      "children": []
    }
  ]
}
```

### Get Flat File List

```
GET /api/googledrive/files?folderId={folderId}
```

**Parameters:**
- `folderId` (optional): The Google Drive folder ID. If not provided, uses the root folder.

**Response:**
```json
[
  {
    "id": "folder-id",
    "name": "Folder Name",
    "mimeType": "application/vnd.google-apps.folder",
    "isFolder": true,
    "modifiedTime": "2024-01-15T10:30:00Z",
    "size": null,
    "children": []
  },
  {
    "id": "file-id",
    "name": "document.pdf",
    "mimeType": "application/pdf",
    "isFolder": false,
    "modifiedTime": "2024-01-15T10:30:00Z",
    "size": 1024000,
    "children": []
  }
]
```

## Finding Folder IDs

You can find a Google Drive folder ID from its URL:
```
https://drive.google.com/drive/folders/{FOLDER_ID}
```

## Example Usage

```bash
# Get structure of root folder
curl https://localhost:5001/api/googledrive/structure

# Get structure of specific folder
curl https://localhost:5001/api/googledrive/structure?folderId=1AbCdEfGhIjKlMnOpQrStUvWxYz

# Get flat list of all files
curl https://localhost:5001/api/googledrive/files?folderId=1AbCdEfGhIjKlMnOpQrStUvWxYz
```

## Notes

- The service uses read-only access to Google Drive
- Large folder structures may take time to retrieve
- The hierarchical endpoint recursively fetches all subfolders
- The flat list endpoint returns all files without hierarchy
