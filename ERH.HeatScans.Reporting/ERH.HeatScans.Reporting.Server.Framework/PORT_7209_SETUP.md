# HTTPS Port 7209 Configuration Guide

## Overview

This guide helps you configure the ERH.HeatScans.Reporting.Server.Framework project to run on **HTTPS port 7209** for debugging.

## Current Configuration

The project is **already configured** to use port 7209 in the following files:

### 1. Project File (.csproj)
```xml
<IISExpressSSLPort>44300</IISExpressSSLPort>  <!-- NEEDS UPDATE TO 7209 -->

<WebProjectProperties>
  <DevelopmentServerPort>7209</DevelopmentServerPort>
  <IISUrl>https://localhost:7209/</IISUrl>
  <IISAppRootUrl>https://localhost:7209/</IISAppRootUrl>
</WebProjectProperties>
```

### 2. CORS Configuration
Currently allows:
- `https://localhost:49806` (Angular default)
- `https://localhost:5173` (Vite dev server)

## Quick Setup

### Option 1: Automated Script (Recommended)

1. **Close Visual Studio completely**

2. Run the configuration script:
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\ConfigurePort7209.ps1
```

3. **Open Visual Studio**

4. Press **F5** to start debugging

### Option 2: Manual Configuration

If Visual Studio is running, you need to manually update the project file:

1. **Close Visual Studio**

2. Open `ERH.HeatScans.Reporting.Server.Framework.csproj` in a text editor

3. Find this line:
```xml
<IISExpressSSLPort>44300</IISExpressSSLPort>
```

4. Change it to:
```xml
<IISExpressSSLPort>7209</IISExpressSSLPort>
```

5. Save and close the file

6. **Open Visual Studio** and load the solution

7. Press **F5** to start debugging

## Verification Steps

### 1. Check Project Properties
- Right-click project ? **Properties**
- Go to **Web** tab
- Under **Servers** section:
  - Project Url should show: `https://localhost:7209/`
  - Should say "Use IIS Express"

### 2. Test the Endpoints

Once running, test these URLs in your browser or Postman:

**Service Account Endpoints:**
```
GET https://localhost:7209/api/googledrive/structure
GET https://localhost:7209/api/googledrive/files
```

**User-Authenticated Endpoints:**
```
GET https://localhost:7209/api/user/googledrive/structure
GET https://localhost:7209/api/user/googledrive/files
```
(Requires Authorization header with Bearer token)

### 3. Check IIS Express

Look in the system tray (bottom-right corner of Windows):
- You should see the IIS Express icon
- Right-click it and select "Show All Applications"
- You should see your application running on `https://localhost:7209`

## SSL Certificate

### First Run
The first time you run the application on port 7209, you may see:

1. **IIS Express Development Certificate Prompt**
   - Click **Yes** to trust the certificate

2. **Windows Security Alert**
   - Click **Allow access** to allow IIS Express through the firewall

### If Certificate Issues Occur

If you get SSL certificate errors:

```powershell
# Remove old certificate binding
netsh http delete sslcert ipport=0.0.0.0:7209

# IIS Express will recreate it on next run
```

Or use the IIS Express SSL repair tool:
```powershell
# From Developer Command Prompt
"%ProgramFiles%\IIS Express\IisExpressAdminCmd.exe" setupsslUrl -url:https://localhost:7209/ -UseSelfSigned
```

## Update Angular Client

After changing the port, update your Angular client service:

**File:** `erh.heatscans.reporting.client/src/app/services/google-drive.service.ts`

```typescript
export class GoogleDriveService {
  private baseUrl = 'https://localhost:7209/api/user/googledrive';
  
  // ...rest of the code
}
```

## CORS Configuration

The server is configured to allow CORS from:
- `https://localhost:49806` (Angular CLI default)
- `https://localhost:5173` (Vite dev server)

If you need to add more origins, update:

**File:** `App_Start/WebApiConfig.cs`

```csharp
var cors = new EnableCorsAttribute(
    origins: "https://localhost:49806,https://localhost:5173,https://localhost:7209",
    headers: "*",
    methods: "*")
{
    SupportsCredentials = true
};
```

And update `Global.asax.cs`:

```csharp
private string GetAllowedOrigin()
{
    var origin = HttpContext.Current.Request.Headers["Origin"];
    var allowedOrigins = new[] { 
        "https://localhost:49806", 
        "https://localhost:5173",
        "https://localhost:7209"  // Add this
    };
    // ...rest of code
}
```

## Troubleshooting

### Issue: Port 7209 is already in use

**Check what's using the port:**
```powershell
netstat -ano | findstr :7209
```

**Find the process:**
```powershell
Get-Process -Id <PID>
```

**Stop the process:**
- If it's IIS Express, kill from system tray
- Or: `Stop-Process -Id <PID> -Force`

### Issue: "Unable to launch IIS Express"

**Solution 1: Delete .vs folder**
```powershell
Remove-Item -Recurse -Force .\.vs
```

**Solution 2: Reset IIS Express configuration**
```powershell
# Close Visual Studio
Remove-Item -Recurse -Force "$env:USERPROFILE\Documents\IISExpress\config\applicationhost.config"
# Restart Visual Studio - it will regenerate the config
```

### Issue: Application opens on wrong port

**Check these locations:**
1. Project Properties ? Web ? Project Url
2. .csproj file ? `<IISExpressSSLPort>` and `<IISUrl>`
3. .vs folder ? applicationhost.config (delete this folder to reset)

### Issue: 404 on all endpoints

**Verify Web API is configured:**
1. Check `Global.asax.cs` has `GlobalConfiguration.Configure(WebApiConfig.Register);`
2. Check controllers have `[RoutePrefix("api/...")]` attributes
3. Check routes in `WebApiConfig.cs`

### Issue: CORS errors in browser

**Check browser console for specific error**, then:

1. Verify the Origin header matches an allowed origin
2. Check both `WebApiConfig.cs` and `Global.asax.cs` CORS settings
3. Check `Web.config` CORS headers
4. Use browser dev tools to inspect request/response headers

## Testing Commands

### PowerShell REST Tests

```powershell
# Test structure endpoint
Invoke-RestMethod -Uri "https://localhost:7209/api/googledrive/structure" -Method Get

# Test with authorization
$token = "YOUR_GOOGLE_ACCESS_TOKEN"
$headers = @{ "Authorization" = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:7209/api/user/googledrive/structure" -Method Get -Headers $headers
```

### cURL Tests

```bash
# Test structure endpoint
curl -k https://localhost:7209/api/googledrive/structure

# Test with authorization
curl -k -H "Authorization: Bearer YOUR_TOKEN" https://localhost:7209/api/user/googledrive/structure
```

## Configuration Files Summary

| File | Purpose | Port Configuration |
|------|---------|-------------------|
| `.csproj` | Project settings | `<IISExpressSSLPort>7209</IISExpressSSLPort>` |
| `WebApiConfig.cs` | CORS & Routes | Allowed origins list |
| `Global.asax.cs` | App startup & CORS | Allowed origins list |
| `Web.config` | IIS settings | CORS headers |

## Port Change Checklist

When changing from default (44300) to 7209:

- [ ] Close Visual Studio completely
- [ ] Update `.csproj` ? `<IISExpressSSLPort>7209</IISExpressSSLPort>`
- [ ] Delete `.vs` folder (optional but recommended)
- [ ] Open Visual Studio
- [ ] Clean solution (Build ? Clean Solution)
- [ ] Rebuild solution (Build ? Rebuild Solution)
- [ ] Press F5 to start debugging
- [ ] Verify URL is `https://localhost:7209/`
- [ ] Test API endpoints
- [ ] Update Angular client baseUrl if needed

## Quick Reference

### Default Ports
- **Old/Default:** `https://localhost:44300/`
- **New/Target:** `https://localhost:7209/`

### API Endpoints
```
https://localhost:7209/api/googledrive/structure
https://localhost:7209/api/googledrive/files
https://localhost:7209/api/user/googledrive/structure
https://localhost:7209/api/user/googledrive/files
```

### Key Files to Update
1. `ERH.HeatScans.Reporting.Server.Framework.csproj`
2. `erh.heatscans.reporting.client/src/app/services/google-drive.service.ts`

## Success Criteria

? Project configured for port 7209 when:

1. ? Visual Studio shows `https://localhost:7209/` in address bar
2. ? IIS Express system tray shows application on port 7209
3. ? API endpoints respond on `https://localhost:7209/api/...`
4. ? No SSL certificate errors
5. ? CORS works from Angular client
6. ? No port conflict errors

## Additional Resources

- **ConfigurePort7209.ps1** - Automated configuration script
- **CHECKLIST.md** - Complete setup checklist
- **README.md** - Project overview
- **NUGET_TROUBLESHOOTING.md** - Package restoration help

---

**Status:** Configuration instructions complete
**Target:** HTTPS on port 7209
**Current:** Project file updated, ready for Visual Studio restart
