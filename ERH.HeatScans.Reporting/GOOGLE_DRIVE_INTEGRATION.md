# Google Drive Integration - Quick Start

## Summary

I've added a complete Google OAuth login system to your Angular app that enables users to sign in with their Google credentials and access their Google Drive files through the API.

## What Was Created

### Backend (.NET 10)

1. **Services**:
   - `UserGoogleDriveService.cs` - Handles Google Drive operations using user OAuth tokens
   - `GoogleDriveService.cs` - Existing service (for service account access)

2. **API Endpoints**:
   - `GET /api/user/googledrive/structure` - Get hierarchical folder/file structure (requires auth)
   - `GET /api/user/googledrive/files` - Get flat list of files (requires auth)

3. **Authentication**:
   - JWT Bearer authentication configured for Google OAuth
   - CORS configured for local development
   - Authorization required for user endpoints

### Frontend (Angular)

1. **Components**:
   - `LoginComponent` - Handles Google Sign-In UI
   - `DriveBrowserComponent` - Displays Google Drive structure
   - `DriveItemComponent` - Renders individual files/folders in a tree view

2. **Services**:
   - `AuthService` - Manages Google authentication and tokens
   - `GoogleDriveService` - Communicates with backend API

3. **Models**:
   - `google-auth.model.ts` - Authentication interfaces
   - `google-drive.model.ts` - Drive file/folder interfaces

## Setup Instructions

### 1. Install npm Package

```bash
cd erh.heatscans.reporting.client
npm install --save @types/google.accounts
```

### 2. Get Google OAuth Credentials

Follow the detailed instructions in `GOOGLE_OAUTH_SETUP.md` to:
1. Create a Google Cloud project
2. Enable Google Drive API
3. Configure OAuth consent screen
4. Create OAuth 2.0 Client ID

### 3. Configure Client ID

Replace `YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com` in:

**Backend**: `appsettings.json`
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "123456789-abc123def456.apps.googleusercontent.com"
    }
  }
}
```

**Frontend**: 
- `src/environments/environment.ts`
- `src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: false,
  googleClientId: '123456789-abc123def456.apps.googleusercontent.com'
};
```

### 4. Run the Application

```bash
# Terminal 1 - Backend
dotnet run --project ERH.HeatScans.Reporting.Server

# Terminal 2 - Frontend
cd erh.heatscans.reporting.client
npm start
```

Navigate to `https://localhost:49806`

## How It Works

1. **User clicks "Sign in with Google"**
   - Google OAuth popup appears
   - User grants permissions to read Google Drive

2. **Authentication Flow**:
   - User receives an ID token (for identity verification)
   - User receives an access token (for API calls)
   - Both tokens are stored in localStorage

3. **API Calls**:
   - Frontend includes access token in Authorization header
   - Backend validates the token
   - Backend uses token to call Google Drive API on behalf of user
   - Results returned to frontend

4. **Drive Browser**:
   - Displays hierarchical folder/file structure
   - Shows file icons based on MIME type
   - Shows file sizes and modification dates
   - Folders can be expanded/collapsed

## Features

? Google OAuth authentication  
? Secure token-based API calls  
? Hierarchical folder/file tree view  
? File metadata display (size, date, type)  
? Read-only access to Google Drive  
? Automatic token refresh  
? User profile display  
? Sign out functionality  

## Security Notes

- Uses read-only scope for Google Drive
- Tokens stored in localStorage (consider more secure storage for production)
- CORS configured for development domains
- JWT Bearer authentication on backend
- HTTPS required for production

## Next Steps

1. **Customize UI**: Update styles to match your brand
2. **Add Features**:
   - File download
   - File search
   - Folder filtering
   - Pagination for large folders
3. **Production Setup**:
   - Configure production OAuth client
   - Set up proper CORS for production domain
   - Use environment variables for sensitive config
   - Implement secure token storage
4. **Error Handling**:
   - Add better error messages
   - Implement retry logic
   - Handle token expiration

## Files Modified/Created

### Backend
- ?? `Program.cs` - Added authentication, CORS, new endpoints
- ?? `ERH.HeatScans.Reporting.Server.csproj` - Added JWT package
- ?? `appsettings.json` - Added Google Client ID config
- ? `Services/UserGoogleDriveService.cs` - New service
- ? `GOOGLE_OAUTH_SETUP.md` - Setup guide

### Frontend
- ?? `app.module.ts` - Registered new components
- ?? `app.component.ts` - Simplified
- ?? `app.component.html` - Updated layout
- ?? `app.component.css` - Simplified styles
- ?? `index.html` - Added Google script
- ?? `styles.css` - Added global styles
- ? `models/google-auth.model.ts` - Auth interfaces
- ? `models/google-drive.model.ts` - Drive interfaces
- ? `services/auth.service.ts` - Authentication service
- ? `services/google-drive.service.ts` - Drive API service
- ? `components/login/` - Login component
- ? `components/drive-browser/` - Drive browser component
- ? `components/drive-item/` - Drive item component
- ? `environments/environment.ts` - Dev config
- ? `environments/environment.prod.ts` - Prod config

## Support

For detailed OAuth setup instructions, see `GOOGLE_OAUTH_SETUP.md`
