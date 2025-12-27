# IIS Deployment Guide

This guide explains how to deploy the ERH.HeatScans.Reporting.Server.Framework application to local IIS with HTTPS.

## Prerequisites

- Windows 10/11 or Windows Server
- IIS installed with ASP.NET 4.8 support
- Visual Studio 2019 or later
- Administrator access

## Quick Setup (Automated)

1. **Run the Setup Script** (as Administrator):
   ```powershell
   .\Setup-IIS.ps1
   ```

   Optional parameters:
   ```powershell
   .\Setup-IIS.ps1 `
       -SiteName "ERH.HeatScans.Reporting" `
       -AppPoolName "ERH.HeatScans.Reporting.AppPool" `
       -PhysicalPath "C:\inetpub\wwwroot\ERH.HeatScans.Reporting" `
       -HostName "localhost" `
       -HttpsPort 443 `
       -CertificateFriendlyName "ERH.HeatScans.LocalDev"
   ```

2. **Publish the Application**:
   - In Visual Studio, right-click on `ERH.HeatScans.Reporting.Server.Framework` project
   - Select **Publish**
   - Choose the **LocalIIS** profile
   - Click **Publish**

3. **Copy Configuration Files**:
   ```powershell
   Copy-Item ".\google-credentials.json" "C:\inetpub\wwwroot\ERH.HeatScans.Reporting\"
   ```

4. **Set Environment Variables** (if needed):
   ```powershell
   .\SetEnvVars.ps1
   ```

5. **Test the Deployment**:
   - Open browser to `https://localhost/api/folders-and-files/structure`
   - Or use the HTTP file: `UserGoogleDrive.http`

## Manual Setup

If you prefer to set up IIS manually, follow these steps:

### 1. Create Self-Signed Certificate

Using PowerShell (as Administrator):
```powershell
$cert = New-SelfSignedCertificate `
    -DnsName "localhost" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "ERH.HeatScans.LocalDev" `
    -NotAfter (Get-Date).AddYears(5)

# Trust the certificate
$rootStore = Get-Item "Cert:\LocalMachine\Root"
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close()
```

Or using IIS Manager:
1. Open IIS Manager (`inetmgr`)
2. Click server name ? **Server Certificates**
3. Click **Create Self-Signed Certificate**
4. Name: `ERH.HeatScans.LocalDev`
5. Certificate store: **Web Hosting**

### 2. Create Application Pool

1. In IIS Manager, click **Application Pools**
2. Click **Add Application Pool**
3. Settings:
   - Name: `ERH.HeatScans.Reporting.AppPool`
   - .NET CLR version: `v4.0`
   - Managed pipeline mode: `Integrated`
   - Start immediately: ?

4. Right-click the new pool ? **Advanced Settings**:
   - Start Mode: `AlwaysRunning`
   - Idle Timeout: `0` (disable)
   - Periodic Restart Time: `0` (disable)
   - Enable 32-Bit Applications: `False`

### 3. Create Website

1. In IIS Manager, right-click **Sites** ? **Add Website**
2. Settings:
   - Site name: `ERH.HeatScans.Reporting`
   - Application pool: `ERH.HeatScans.Reporting.AppPool`
   - Physical path: `C:\inetpub\wwwroot\ERH.HeatScans.Reporting`
   - Binding:
     - Type: `https`
     - IP address: `All Unassigned`
     - Port: `443`
     - Host name: `localhost` (optional)
     - SSL certificate: `ERH.HeatScans.LocalDev`

### 4. Set Permissions

Grant read access to the application pool identity:

```powershell
$path = "C:\inetpub\wwwroot\ERH.HeatScans.Reporting"
$acl = Get-Acl $path

$identities = @("IIS_IUSRS", "IUSR", "IIS APPPOOL\ERH.HeatScans.Reporting.AppPool")
foreach ($identity in $identities) {
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $identity, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
    )
    $acl.SetAccessRule($rule)
}
Set-Acl $path $acl
```

### 5. Publish Application

#### Option A: Using Visual Studio
1. Right-click project ? **Publish**
2. Select **LocalIIS** profile
3. Click **Publish**

#### Option B: Using MSBuild
```powershell
msbuild ERH.HeatScans.Reporting.Server.Framework.csproj `
    /p:Configuration=Release `
    /p:DeployOnBuild=true `
    /p:PublishProfile=LocalIIS
```

#### Option C: Using dotnet CLI (if applicable)
```powershell
dotnet publish -c Release /p:PublishProfile=LocalIIS
```

### 6. Post-Deployment Configuration

1. **Copy google-credentials.json**:
   ```powershell
   Copy-Item ".\google-credentials.json" "C:\inetpub\wwwroot\ERH.HeatScans.Reporting\"
   ```

2. **Set Environment Variables** (if needed):
   - Open IIS Manager
   - Select the website
   - Double-click **Configuration Editor**
   - Section: `system.webServer/aspNetCore` or use Application Settings
   - Add environment variables as needed

3. **Verify Web.config**:
   - Ensure `google-credentials.json` path is correct
   - Verify `GoogleMapsApiKey` is set (via environment or config)

## Publish Profiles

### LocalIIS (File System)
Publishes to `C:\inetpub\wwwroot\ERH.HeatScans.Reporting` using file copy.
- Fast deployment
- Manual IIS setup required
- Good for local development

### LocalIIS-MSDeploy
Publishes using Web Deploy (MSDeploy) directly to IIS.
- Automatic deployment
- Creates/updates site automatically
- Requires Web Deploy installed

## Troubleshooting

### 403 Forbidden Error
- **Cause**: Insufficient permissions
- **Fix**: Grant IIS_IUSRS and application pool identity read permissions

### 500 Internal Server Error
- **Cause**: Multiple possible causes
- **Fix**: 
  1. Check Event Viewer (Windows Logs ? Application)
  2. Enable detailed errors: `<customErrors mode="Off"/>`
  3. Check application pool identity has proper permissions
  4. Verify .NET 4.8 is installed

### Certificate Not Trusted
- **Cause**: Certificate not in Trusted Root store
- **Fix**: Run the trust certificate commands in Setup-IIS.ps1

### HTTP Error 404.2 - Not Found
- **Cause**: ISAPI and CGI Restrictions
- **Fix**: 
  1. Open IIS Manager
  2. Server level ? **ISAPI and CGI Restrictions**
  3. Ensure ASP.NET 4.0 is **Allowed**

### Application Pool Crashes
- **Cause**: Missing dependencies or configuration errors
- **Fix**:
  1. Check Event Viewer
  2. Verify all NuGet packages are restored
  3. Check `google-credentials.json` exists
  4. Verify all referenced projects are published

## Configuration Options

### Web.config Settings

#### Debug Mode
```xml
<compilation debug="false" targetFramework="4.8">
```
Set to `false` for production (better performance).

#### Custom Errors
```xml
<customErrors mode="RemoteOnly" />
```
- `Off`: Show detailed errors (development only)
- `RemoteOnly`: Show detailed errors locally, generic remotely
- `On`: Always show generic errors (production)

#### Request Size Limits
```xml
<httpRuntime maxRequestLength="52428800" executionTimeout="3600" />
<requestFiltering>
  <requestLimits maxAllowedContentLength="52428800" />
</requestFiltering>
```
Current: 50MB max upload, 1 hour timeout

#### HTTPS Redirect
Uncomment in Web.config to force HTTPS:
```xml
<rewrite>
  <rules>
    <rule name="HTTPS Redirect" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="off" />
      </conditions>
      <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" />
    </rule>
  </rules>
</rewrite>
```

## URLs and Testing

### API Endpoints
- Structure: `https://localhost/api/folders-and-files/structure`
- Files: `https://localhost/api/folders-and-files/files`
- Image: `https://localhost/api/folders-and-files/image?fileId={id}`
- Maps: `https://localhost/api/maps/...`

### Using HTTP Client
Update `http-client.env.json`:
```json
{
  "dev": {
    "BaseUrl": "https://localhost",
    "AccessToken": "your-token-here",
    "AddressFolderId": "folder-id-here"
  }
}
```

Then use `UserGoogleDrive.http` for testing.

## Production Considerations

1. **Change Debug Mode**: Set `debug="false"` in Web.config
2. **Use Valid Certificate**: Replace self-signed cert with proper SSL certificate
3. **Set Custom Errors**: Use `RemoteOnly` or `On`
4. **Enable HTTPS Redirect**: Uncomment URL rewrite rule
5. **Set Application Pool**: Configure recycling, idle timeout appropriately
6. **Monitor Logs**: Check IIS logs and Windows Event Viewer
7. **Backup Configuration**: Save IIS configuration and certificates

## Additional Resources

- [IIS Configuration Reference](https://docs.microsoft.com/en-us/iis/configuration/)
- [ASP.NET Web.config Reference](https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/visual-studio-web-deployment/)
- [IIS URL Rewrite](https://docs.microsoft.com/en-us/iis/extensions/url-rewrite-module/)

## Support

For issues specific to this application, check:
1. `ENVIRONMENT_VARIABLES.md` - Environment setup
2. Project README - Application-specific documentation
3. Repository issues - Known problems and solutions
