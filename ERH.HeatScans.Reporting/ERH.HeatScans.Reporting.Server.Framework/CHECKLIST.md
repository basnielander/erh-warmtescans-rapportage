# Setup and Testing Checklist

## ? Pre-Setup Checklist

### System Requirements
- [ ] Windows operating system
- [ ] .NET Framework 4.8.1 Developer Pack installed
- [ ] Visual Studio 2019 or later installed
- [ ] IIS Express available (comes with Visual Studio)
- [ ] PowerShell 5.0 or later

### Project Files
- [ ] All files from `ERH.HeatScans.Reporting.Server.Framework/` directory present
- [ ] Google credentials file available (`google-credentials.json`)

## ?? Initial Setup

### 1. Run Setup Script
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\Setup.ps1
```

- [ ] Script executed successfully
- [ ] .NET Framework 4.8.1 detected
- [ ] Google credentials copied (if applicable)

### 2. Open in Visual Studio
- [ ] Open solution in Visual Studio
- [ ] Project loads without errors
- [ ] All files visible in Solution Explorer

### 3. NuGet Package Restoration

**Option 1: Use Automated Script (Recommended)**
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\RestorePackages.ps1
```
Or on Windows:
```cmd
cd ERH.HeatScans.Reporting.Server.Framework
RestorePackages.bat
```

**Option 2: Visual Studio UI**
- [ ] Right-click solution ? "Restore NuGet Packages"
- [ ] All packages restored successfully
- [ ] No package restoration errors in output window

**Option 3: Package Manager Console**
In Visual Studio:
- [ ] Tools ? NuGet Package Manager ? Package Manager Console
- [ ] Run: `Update-Package -ProjectName ERH.HeatScans.Reporting.Server.Framework -Reinstall`
- [ ] Wait for completion

Expected packages:
- [ ] Microsoft.AspNet.WebApi
- [ ] Microsoft.AspNet.WebApi.Cors
- [ ] Google.Apis.Drive.v3
- [ ] Unity
- [ ] Unity.AspNet.WebApi
- [ ] Newtonsoft.Json
- [ ] System.IdentityModel.Tokens.Jwt

**If restoration fails, see NUGET_TROUBLESHOOTING.md**

### 4. Configuration
- [ ] `Web.config` exists
- [ ] Google credentials path configured correctly
- [ ] OAuth client ID configured
- [ ] CORS origins configured for your client

Edit if needed:
```xml
<appSettings>
  <add key="GoogleDrive:CredentialPath" value="./google-credentials.json" />
  <add key="Authentication:Google:ClientId" value="YOUR_CLIENT_ID" />
</appSettings>
```

### 5. Google Credentials
- [ ] `google-credentials.json` file exists in project root
- [ ] File is valid JSON
- [ ] Service account has Google Drive permissions
- [ ] File set to "Copy to Output Directory" = "Copy always"

To verify in Visual Studio:
1. Right-click `google-credentials.json`
2. Select "Properties"
3. Set "Copy to Output Directory" to "Copy always"

## ?? Build Process

### 1. Clean Solution
- [ ] Build ? Clean Solution
- [ ] No errors reported

### 2. Rebuild Solution
- [ ] Build ? Rebuild Solution (or Ctrl+Shift+B)
- [ ] Build succeeded
- [ ] No errors (warnings OK)
- [ ] Output shows: "Build succeeded"

Expected output:
```
========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
```

### 3. Verify Binary Output
- [ ] `bin\` directory created
- [ ] `ERH.HeatScans.Reporting.Server.Framework.dll` exists
- [ ] All dependencies copied to bin

## ?? Running the Application

### 1. Start Debugging
- [ ] Press F5 (or Debug ? Start Debugging)
- [ ] IIS Express starts
- [ ] Browser opens (may show empty page, that's OK)
- [ ] No exceptions in Visual Studio

### 2. Verify Server Running
- [ ] Application runs on `https://localhost:44300/`
- [ ] No SSL certificate errors (or accept dev certificate)
- [ ] Server responds to requests

### 3. Check Output Window
- [ ] No errors in Output window
- [ ] No exceptions in Debug output

## ?? API Testing

### Test 1: Service Account Endpoint (Structure)
```
GET https://localhost:44300/api/googledrive/structure
```

Using PowerShell:
```powershell
Invoke-RestMethod -Uri "https://localhost:44300/api/googledrive/structure" -Method Get
```

Expected result:
- [ ] Status 200 OK
- [ ] JSON response with folder structure
- [ ] Contains `id`, `name`, `mimeType`, `children`

### Test 2: Service Account Endpoint (Files)
```
GET https://localhost:44300/api/googledrive/files
```

Using PowerShell:
```powershell
Invoke-RestMethod -Uri "https://localhost:44300/api/googledrive/files" -Method Get
```

Expected result:
- [ ] Status 200 OK
- [ ] JSON array of files
- [ ] Contains file metadata

### Test 3: User Endpoint (with Bearer Token)
```
GET https://localhost:44300/api/user/googledrive/structure
Headers: Authorization: Bearer {your-token}
```

Using PowerShell:
```powershell
$token = "YOUR_GOOGLE_ACCESS_TOKEN"
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "https://localhost:44300/api/user/googledrive/structure" `
    -Method Get -Headers $headers
```

Expected result:
- [ ] With valid token: Status 200 OK with data
- [ ] Without token: Status 401 Unauthorized

### Test 4: CORS Preflight
```
OPTIONS https://localhost:44300/api/googledrive/structure
Origin: https://localhost:49806
```

Expected response headers:
- [ ] `Access-Control-Allow-Origin: https://localhost:49806`
- [ ] `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
- [ ] `Access-Control-Allow-Headers: Content-Type, Authorization`
- [ ] `Access-Control-Allow-Credentials: true`

## ?? Client Integration

### 1. Update Angular Client
In `erh.heatscans.reporting.client/src/app/services/google-drive.service.ts`:

```typescript
private baseUrl = 'https://localhost:44300/api/user/googledrive';
```

- [ ] Base URL updated
- [ ] Angular app recompiled
- [ ] No TypeScript errors

### 2. Test from Client
- [ ] Start Angular dev server (`npm start`)
- [ ] Navigate to application in browser
- [ ] Sign in with Google
- [ ] Google Drive browser component loads
- [ ] Folder structure displays correctly
- [ ] No CORS errors in browser console

### 3. Browser Console Check
Press F12 in browser:
- [ ] No CORS errors
- [ ] API requests succeed (200 status)
- [ ] Data displays correctly

## ?? Functionality Verification

### Core Features
- [ ] Service account authentication works
- [ ] User OAuth authentication works
- [ ] Folder structure retrieval works
- [ ] File listing works
- [ ] Recursive folder traversal works
- [ ] CORS headers present
- [ ] Error handling works (test with invalid folder ID)

### Test Scenarios

#### Scenario 1: Valid Folder
- [ ] Request with valid folder ID succeeds
- [ ] Returns correct folder structure
- [ ] Children populated correctly

#### Scenario 2: Invalid Folder
- [ ] Request with invalid folder ID fails gracefully
- [ ] Returns 500 error
- [ ] Error message included in response

#### Scenario 3: No Authentication
- [ ] User endpoints without token return 401
- [ ] Service account endpoints work without token

#### Scenario 4: Root Folder
- [ ] Request without folder ID uses default
- [ ] Default folder ID: `1A9-OGvD5LDPzFggsPNG7PF3pKl9xcHvQ`

## ?? Troubleshooting

### Build Errors
If build fails:
- [ ] Check .NET Framework 4.8.1 is installed
- [ ] Restore NuGet packages again
- [ ] Clean and rebuild
- [ ] Check Error List window for details

### NuGet Package Errors
If packages fail to restore:
- [ ] Check internet connection
- [ ] Clear NuGet cache: `dotnet nuget locals all --clear`
- [ ] Update NuGet Package Manager
- [ ] Check `packages.config` is present

### Google API Errors
If Google API fails:
- [ ] Verify credentials file exists
- [ ] Check credentials file path in Web.config
- [ ] Verify service account has Drive API enabled
- [ ] Check service account permissions on folders

### CORS Errors
If CORS fails:
- [ ] Check allowed origins in `WebApiConfig.cs`
- [ ] Verify client origin exactly matches
- [ ] Check browser console for specific error
- [ ] Ensure CORS package is installed

### 401 Unauthorized
If getting 401 errors:
- [ ] Check Authorization header format
- [ ] Verify token is valid (not expired)
- [ ] Check token includes "Bearer " prefix
- [ ] Test service account endpoints (no auth needed)

### 404 Not Found
If endpoints return 404:
- [ ] Verify URL is correct (https://localhost:44300)
- [ ] Check route configuration in controllers
- [ ] Verify WebApiConfig.cs routing setup
- [ ] Check controller RoutePrefix attributes

### 500 Internal Server Error
If getting 500 errors:
- [ ] Check Output window for exception details
- [ ] Enable custom errors: `<customErrors mode="Off"/>`
- [ ] Check Windows Event Log
- [ ] Verify Google credentials are valid

## ?? Documentation Review

### Files to Review
- [ ] Read `README.md` - Overview and setup
- [ ] Read `MIGRATION_GUIDE.md` - Detailed comparisons
- [ ] Read `CONVERSION_SUMMARY.md` - High-level summary
- [ ] Read `QUICK_REFERENCE.md` - Side-by-side reference
- [ ] Read this checklist

## ?? Final Verification

### All Systems Go
- [ ] Server builds successfully
- [ ] Server runs without errors
- [ ] Service account endpoints work
- [ ] User endpoints work with authentication
- [ ] CORS configured correctly
- [ ] Client integration works
- [ ] No console errors
- [ ] Google Drive data displays correctly

## ?? Deployment Readiness

### Pre-Deployment
- [ ] All tests passing
- [ ] Configuration reviewed
- [ ] Production credentials prepared
- [ ] IIS server prepared (.NET Framework 4.8.1 installed)
- [ ] SSL certificate ready

### Deployment Steps
- [ ] Publish project from Visual Studio
- [ ] Copy files to IIS web directory
- [ ] Create IIS application
- [ ] Configure application pool (.NET Framework 4.8)
- [ ] Copy google-credentials.json
- [ ] Update Web.config for production
- [ ] Test deployed application

## ? Success Criteria

You've successfully completed the migration when:

1. ? Project builds without errors
2. ? Server runs on https://localhost:44300/
3. ? All API endpoints respond correctly
4. ? CORS headers present
5. ? Client can communicate with server
6. ? Google Drive integration works
7. ? No errors in browser console
8. ? No errors in Visual Studio output

## ?? Getting Help

If you encounter issues:

1. Check the troubleshooting section above
2. Review error messages in:
   - Visual Studio Error List
   - Visual Studio Output window
   - Browser console (F12)
   - Windows Event Log
3. Review documentation files
4. Check configuration settings
5. Verify all prerequisites met

## ?? Performance Baseline

After successful setup, establish baseline metrics:

- [ ] Measure cold start time: _____ seconds
- [ ] Measure request time (structure): _____ ms
- [ ] Measure request time (files): _____ ms
- [ ] Check memory usage: _____ MB
- [ ] Monitor CPU usage under load: _____ %

## ?? Completion

Date completed: _______________

Completed by: _______________

Notes:
_____________________________________
_____________________________________
_____________________________________

**Congratulations! Your .NET Framework 4.8.1 migration is complete!** ??
