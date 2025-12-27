# IIS Setup - Quick Start Guide

## What Was Created

The following files have been added to your project for IIS deployment:

### 1. Publish Profiles
- `Properties/PublishProfiles/LocalIIS.pubxml` - File system publish
- `Properties/PublishProfiles/LocalIIS-MSDeploy.pubxml` - Web Deploy publish

### 2. Configuration
- `Web.Release.config` - Production transformation file
- Updated `Web.config` with IIS-specific optimizations

### 3. Scripts
- `Setup-IIS.ps1` - Automated IIS setup script
- `IIS-DEPLOYMENT.md` - Complete deployment guide

## Quick Start (3 Steps)

### Step 1: Run Setup Script
Open PowerShell as Administrator in the project directory:
```powershell
cd "C:\Projects\ERH\warmtescans-rapportage\ERH.HeatScans.Reporting\ERH.HeatScans.Reporting.Server.Framework"
.\Setup-IIS.ps1
```

This will:
- Create a self-signed certificate named "ERH.HeatScans.LocalDev"
- Trust the certificate
- Create IIS application pool "ERH.HeatScans.Reporting.AppPool"
- Create IIS website "ERH.HeatScans.Reporting"
- Configure permissions
- Bind HTTPS certificate

### Step 2: Publish Application
In Visual Studio:
1. Right-click on `ERH.HeatScans.Reporting.Server.Framework` project
2. Select **Publish**
3. Click **New Profile** or select **LocalIIS**
4. Click **Publish**

### Step 3: Copy Credentials
```powershell
Copy-Item "google-credentials.json" "C:\inetpub\wwwroot\ERH.HeatScans.Reporting\"
```

## Test Your Deployment

1. **Browser**: Navigate to `https://localhost/api/folders-and-files/structure`
2. **HTTP Client**: Update `http-client.env.json`:
   ```json
   {
     "dev": {
       "BaseUrl": "https://localhost",
       "AccessToken": "your-token",
       "AddressFolderId": "your-folder-id"
     }
   }
   ```

## Key Configuration Changes

### Web.config Improvements
- ? Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- ? Static content caching (7 days)
- ? Dynamic content compression
- ? Request size limit: 50MB
- ? Execution timeout: 1 hour
- ? URL rewrite rules (commented, ready to enable)
- ? HTTP error handling (commented, ready to enable)

### Application Pool Settings
- .NET CLR Version: v4.0
- Managed Pipeline: Integrated
- Platform: x64
- Start Mode: AlwaysRunning
- Identity: ApplicationPoolIdentity

## Customization Options

### Change Port
```powershell
.\Setup-IIS.ps1 -HttpsPort 44300
```

### Change Install Path
```powershell
.\Setup-IIS.ps1 -PhysicalPath "D:\websites\ERH.HeatScans.Reporting"
```

### Use Custom Hostname
```powershell
.\Setup-IIS.ps1 -HostName "heatscans.local"
```
Then browse to: `https://heatscans.local/`

## Troubleshooting

### Certificate Trust Issues
Re-run the trust commands:
```powershell
$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object {$_.FriendlyName -eq "ERH.HeatScans.LocalDev"}
$rootStore = Get-Item "Cert:\LocalMachine\Root"
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close()
```

### Permissions Issues
```powershell
$path = "C:\inetpub\wwwroot\ERH.HeatScans.Reporting"
icacls $path /grant "IIS_IUSRS:(OI)(CI)RX" /T
icacls $path /grant "IIS APPPOOL\ERH.HeatScans.Reporting.AppPool:(OI)(CI)RX" /T
```

### See Detailed Errors
Temporarily set in Web.config:
```xml
<customErrors mode="Off"/>
```

## Next Steps

1. ? IIS is configured
2. ? Certificate is trusted
3. ? Application is published
4. ? Set environment variables (if needed)
5. ? Configure Angular app proxy (if needed)
6. ? Test all API endpoints

## Documentation

For complete details, see:
- `IIS-DEPLOYMENT.md` - Full deployment guide
- `ENVIRONMENT_VARIABLES.md` - Environment setup
- `UserGoogleDrive.http` - API testing examples

## Production Deployment

When ready for production:

1. Use a proper SSL certificate (not self-signed)
2. Set `debug="false"` in Web.config
3. Enable HTTPS redirect in Web.config
4. Set `customErrors mode="RemoteOnly"`
5. Review security headers
6. Configure application pool recycling
7. Set up monitoring and logging

## Support

If you encounter issues:
1. Check IIS Manager ? Sites ? ERH.HeatScans.Reporting ? Browse
2. Check Windows Event Viewer ? Application logs
3. Check `IIS-DEPLOYMENT.md` troubleshooting section
4. Review the project's GitHub issues

---

**Summary**: Your IIS deployment is ready! Run `Setup-IIS.ps1` as admin, publish from Visual Studio using the LocalIIS profile, and you're done.
