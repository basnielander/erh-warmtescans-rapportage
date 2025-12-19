# Google OAuth Setup Guide

This guide will help you set up Google OAuth authentication for the ERH Heat Scans Reporting application.

## Prerequisites

- A Google Cloud Platform account
- Access to the Google Cloud Console

## Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "Select a project" ? "New Project"
3. Enter a project name (e.g., "ERH Heat Scans Reporting")
4. Click "Create"

## Step 2: Enable Required APIs

1. In the Google Cloud Console, go to "APIs & Services" ? "Library"
2. Search for and enable the following APIs:
   - **Google Drive API**
   - **Google Sign-In** (included in Google Identity Services)

## Step 3: Configure OAuth Consent Screen

1. Go to "APIs & Services" ? "OAuth consent screen"
2. Choose "External" user type (or "Internal" if using Google Workspace)
3. Click "Create"
4. Fill in the required information:
   - **App name**: ERH Heat Scans Reporting
   - **User support email**: Your email
   - **Developer contact information**: Your email
5. Click "Save and Continue"
6. Add scopes:
   - Click "Add or Remove Scopes"
   - Add `https://www.googleapis.com/auth/drive.readonly`
   - Click "Update"
7. Click "Save and Continue"
8. Review and click "Back to Dashboard"

## Step 4: Create OAuth 2.0 Credentials

1. Go to "APIs & Services" ? "Credentials"
2. Click "Create Credentials" ? "OAuth client ID"
3. Choose "Web application"
4. Configure the client:
   - **Name**: ERH Heat Scans Web Client
   - **Authorized JavaScript origins**:
     - `http://localhost:49806`
     - `https://localhost:49806`
     - `http://localhost:5173`
     - `https://localhost:5173`
     - Add your production domain when ready
   - **Authorized redirect URIs**:
     - `http://localhost:49806`
     - `https://localhost:49806`
     - `http://localhost:5173`
     - `https://localhost:5173`
     - Add your production domain when ready
5. Click "Create"
6. Copy the **Client ID** (it will look like: `123456789-abc123def456.apps.googleusercontent.com`)

## Step 5: Configure the Application

### Backend Configuration

Update `ERH.HeatScans.Reporting.Server/appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_ACTUAL_CLIENT_ID.apps.googleusercontent.com"
    }
  }
}
```

**For production**, use environment variables or `appsettings.Production.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_PRODUCTION_CLIENT_ID.apps.googleusercontent.com"
    }
  }
}
```

### Frontend Configuration

Update the following files with your Client ID:

1. `erh.heatscans.reporting.client/src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  googleClientId: 'YOUR_ACTUAL_CLIENT_ID.apps.googleusercontent.com'
};
```

2. `erh.heatscans.reporting.client/src/environments/environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  googleClientId: 'YOUR_PRODUCTION_CLIENT_ID.apps.googleusercontent.com'
};
```

## Step 6: Install Required npm Package

Run the following command in the Angular project directory:

```bash
cd erh.heatscans.reporting.client
npm install --save @types/google.accounts
```

This will add TypeScript definitions for the Google Identity Services.

## Step 7: Test the Application

1. Start the backend server
2. Start the Angular development server
3. Navigate to `https://localhost:49806`
4. Click the "Sign in with Google" button
5. Grant permissions to access your Google Drive
6. You should see your user profile and be able to browse your Google Drive

## API Endpoints

Once authenticated, the following endpoints are available:

### Get Folder Structure
```
GET /api/user/googledrive/structure?folderId={optional}
Authorization: Bearer {access_token}
```

Returns a hierarchical tree structure of folders and files.

### Get Flat File List
```
GET /api/user/googledrive/files?folderId={optional}
Authorization: Bearer {access_token}
```

Returns a flat list of all files recursively.

## Security Best Practices

1. **Never commit credentials**: Add `appsettings.Production.json` and `environment.prod.ts` to `.gitignore`
2. **Use environment variables** in production for sensitive configuration
3. **Restrict OAuth scopes** to only what's needed (read-only in this case)
4. **Set up proper CORS policies** for production domains
5. **Use HTTPS** in production
6. **Regularly rotate credentials** and review access

## Troubleshooting

### "Google Identity Services not loaded" Error
- Check that the script tag is present in `index.html`
- Ensure the page has fully loaded before initializing

### "Invalid OAuth client" Error
- Verify the Client ID is correct in all configuration files
- Check that the origin is added to Authorized JavaScript origins
- Clear browser cache and try again

### "Access Denied" Error
- Ensure the OAuth consent screen is properly configured
- Verify the required scopes are added
- Check that the user has granted permissions

### CORS Errors
- Verify CORS configuration in `Program.cs`
- Check that the Angular dev server URL is included in CORS origins
- For production, update CORS to include your production domain

## Additional Resources

- [Google Identity Services Documentation](https://developers.google.com/identity/gsi/web/guides/overview)
- [Google Drive API Documentation](https://developers.google.com/drive/api/guides/about-sdk)
- [OAuth 2.0 Best Practices](https://tools.ietf.org/html/rfc6749)
