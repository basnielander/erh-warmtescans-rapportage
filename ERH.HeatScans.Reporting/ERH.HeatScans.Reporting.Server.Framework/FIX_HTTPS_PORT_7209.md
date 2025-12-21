# Fix IIS Express HTTPS on Port 7209

## Quick Fix (Recommended)

**Run as Administrator:**

```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\Setup-IISExpressSSL.ps1
```

This script will:
1. ? Create/find a self-signed certificate for localhost
2. ? Add it to Trusted Root Certification Authorities (eliminates browser warnings)
3. ? Bind the certificate to port 7209
4. ? Configure Windows Firewall rules
5. ? Verify the configuration

## What This Fixes

When IIS Express shows errors like:
- "Unable to launch IIS Express Web Server"
- "Failed to register URL https://localhost:7209/ for site"
- "SSL certificate not found"
- Browser shows "This site can't provide a secure connection"

## Why This Happens

IIS Express requires:
1. A valid SSL certificate in the Local Machine certificate store
2. The certificate bound to the specific port (7209)
3. Proper Windows HTTP.SYS configuration

## Manual Fix (If Script Doesn't Work)

### Step 1: Create Self-Signed Certificate

```powershell
# Run as Administrator
$cert = New-SelfSignedCertificate `
    -DnsName "localhost" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "IIS Express Development Certificate" `
    -NotAfter (Get-Date).AddYears(10)

# Trust the certificate
$thumbprint = $cert.Thumbprint
$certPath = "Cert:\LocalMachine\My\$thumbprint"
$cert = Get-Item $certPath
Export-Certificate -Cert $cert -FilePath "$env:TEMP\localhost.cer"
Import-Certificate -FilePath "$env:TEMP\localhost.cer" -CertStoreLocation Cert:\LocalMachine\Root
Remove-Item "$env:TEMP\localhost.cer"
```

### Step 2: Bind Certificate to Port 7209

```powershell
# Run as Administrator
# Replace {THUMBPRINT} with your certificate thumbprint
netsh http add sslcert ipport=0.0.0.0:7209 certhash={THUMBPRINT} appid={214124cd-d05b-4309-9af9-9caa44b2b74a}
```

### Step 3: Verify Binding

```powershell
netsh http show sslcert ipport=0.0.0.0:7209
```

Should show:
```
IP:port                      : 0.0.0.0:7209
Certificate Hash             : [your cert thumbprint]
Application ID               : {214124cd-d05b-4309-9af9-9caa44b2b74a}
```

### Step 4: Add Firewall Rule

```powershell
# Run as Administrator
New-NetFirewallRule `
    -DisplayName "IIS Express - Port 7209" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 7209 `
    -Action Allow
```

## Troubleshooting

### Certificate Already Exists Error

If you see "SSL certificate add failed, Error: 1312", remove the existing binding first:

```powershell
netsh http delete sslcert ipport=0.0.0.0:7209
```

Then run the setup script again.

### Find Your Certificate Thumbprint

```powershell
Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Subject -match "localhost" } | Select-Object Thumbprint, Subject, NotAfter
```

### Reset IIS Express Configuration

If issues persist:

1. Close Visual Studio
2. Delete the `.vs` folder in your solution directory
3. Run `Setup-IISExpressSSL.ps1` as Administrator
4. Reopen Visual Studio

### Browser Still Shows Security Warning

1. Clear browser cache and SSL state
2. In Chrome: Visit `chrome://restart`
3. In Edge: Settings > Privacy > Clear browsing data > Cached images and files
4. Restart the browser completely

## Verify Everything Works

After running the setup:

1. Open Visual Studio
2. Open the solution
3. Press F5
4. Navigate to https://localhost:7209
5. Should see your API without certificate warnings

## Alternative: Change to Different Port

If you want to use a different port:

1. Edit `ERH.HeatScans.Reporting.Server.Framework.csproj`
   - Change `<IISExpressSSLPort>7209</IISExpressSSLPort>` to your port
   - Change `<IISUrl>https://localhost:7209</IISUrl>` to your port

2. Run: `.\Setup-IISExpressSSL.ps1 -Port YOUR_PORT`

## Common Errors and Solutions

| Error | Solution |
|-------|----------|
| "Access is denied" | Run PowerShell as Administrator |
| "Certificate not found" | Run `Setup-IISExpressSSL.ps1` to create one |
| "Port already in use" | Check with `netstat -ano \| findstr :7209` and kill the process |
| "Cannot bind to port" | Delete old binding: `netsh http delete sslcert ipport=0.0.0.0:7209` |

## Additional Resources

- IIS Express Overview: https://learn.microsoft.com/en-us/iis/extensions/introduction-to-iis-express/
- netsh http commands: https://learn.microsoft.com/en-us/windows/win32/http/netsh-commands-for-http
