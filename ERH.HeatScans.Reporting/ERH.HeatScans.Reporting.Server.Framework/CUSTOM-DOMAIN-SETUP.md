# Custom Domain Setup Guide - test.nielander.nl

This guide explains how to set up IIS with a self-signed certificate for the custom domain `test.nielander.nl`.

## Quick Setup

### Option 1: Full IIS Setup with Custom Domain (Recommended)
Run this to set up everything (certificate + IIS):

```powershell
# As Administrator
.\Setup-IIS-CustomDomain.ps1 -DnsName "test.nielander.nl"
```

### Option 2: Certificate Only
If you just want to create the certificate:

```powershell
# As Administrator
.\Setup-Certificate-TestDomain.ps1
```

Or double-click: `Setup-Certificate-TestDomain.bat` (Run as Administrator)

## What Gets Created

1. **Self-Signed Certificate**
   - Domain: `test.nielander.nl`
   - Also includes: `www.test.nielander.nl`
   - Friendly Name: `ERH.HeatScans.TestDomain`
   - Valid for: 5 years
   - Key Length: 2048-bit RSA
   - Hash Algorithm: SHA256

2. **Certificate Locations**
   - Personal Store: `Cert:\LocalMachine\My`
   - Trusted Root: `Cert:\LocalMachine\Root`

3. **IIS Configuration**
   - Application Pool: `ERH.HeatScans.Reporting.AppPool`
   - Website: `ERH.HeatScans.Reporting`
   - Binding: `https://test.nielander.nl:443`
   - SNI (Server Name Indication): Enabled

4. **Hosts File Entry**
   - `127.0.0.1 test.nielander.nl`

## Detailed Setup Steps

### Step 1: Create Certificate

Run as Administrator:
```powershell
.\Setup-Certificate-TestDomain.ps1
```

The script will:
- Create a self-signed certificate for `test.nielander.nl`
- Add it to the Personal certificate store
- Trust it by adding to Trusted Root store
- Optionally export the certificate
- Optionally configure IIS binding
- Optionally update hosts file

### Step 2: Configure IIS (if not done in Step 1)

#### Automatic Configuration
```powershell
.\Setup-IIS-CustomDomain.ps1 -DnsName "test.nielander.nl"
```

#### Manual Configuration
1. Open IIS Manager (`inetmgr`)
2. Select your site: `ERH.HeatScans.Reporting`
3. Click **Bindings** in Actions panel
4. Add new binding:
   - Type: `https`
   - IP address: `All Unassigned`
   - Port: `443`
   - Host name: `test.nielander.nl`
   - SSL certificate: `ERH.HeatScans.TestDomain`
   - ? Require Server Name Indication

### Step 3: Update Hosts File (if not done automatically)

Edit `C:\Windows\System32\drivers\etc\hosts` (as Administrator):
```
127.0.0.1 test.nielander.nl
```

### Step 4: Publish Application

In Visual Studio:
1. Right-click project ? **Publish**
2. Select **LocalIIS** profile
3. Click **Publish**

### Step 5: Test

Browse to: `https://test.nielander.nl/api/folders-and-files/structure`

## Using with HTTP Client

Update `http-client.env.json`:

```json
{
  "dev": {
    "BaseUrl": "https://test.nielander.nl",
    "AccessToken": "your-google-token",
    "AddressFolderId": "your-folder-id",
    "ImageFileId": "your-image-id"
  }
}
```

Then use `UserGoogleDrive.http` for testing.

## Certificate Management

### View Certificate Details
```powershell
Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" } | Format-List
```

### Export Certificate (with private key)
```powershell
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
$password = ConvertTo-SecureString -String "YourPassword123" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "C:\Temp\test.nielander.nl.pfx" -Password $password
```

### Export Certificate (public key only)
```powershell
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
Export-Certificate -Cert $cert -FilePath "C:\Temp\test.nielander.nl.cer"
```

### Import Certificate on Another Machine
```powershell
# Import to Personal store
Import-PfxCertificate -FilePath "C:\Temp\test.nielander.nl.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password $password

# Trust it
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
$rootStore = Get-Item Cert:\LocalMachine\Root
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close()
```

### Remove Certificate
```powershell
# From Personal store
Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" } | Remove-Item

# From Trusted Root store
Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like "*test.nielander.nl*" } | Remove-Item
```

## IIS Binding Management

### Add Binding (PowerShell)
```powershell
New-WebBinding -Name "ERH.HeatScans.Reporting" `
    -Protocol "https" `
    -Port 443 `
    -HostHeader "test.nielander.nl" `
    -SslFlags 1

$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
$binding = Get-WebBinding -Name "ERH.HeatScans.Reporting" -Protocol "https" -HostHeader "test.nielander.nl"
$binding.AddSslCertificate($cert.Thumbprint, "My")
```

### Remove Binding
```powershell
Remove-WebBinding -Name "ERH.HeatScans.Reporting" -Protocol "https" -HostHeader "test.nielander.nl"
```

### List Bindings
```powershell
Get-WebBinding -Name "ERH.HeatScans.Reporting"
```

## Advanced Configuration

### Multiple Domains on Same Site

Add additional bindings for different domains:

```powershell
# Create certificates for each domain
.\Setup-Certificate-TestDomain.ps1 -DnsName "test2.nielander.nl"
.\Setup-Certificate-TestDomain.ps1 -DnsName "test3.nielander.nl"

# Add bindings
New-WebBinding -Name "ERH.HeatScans.Reporting" -Protocol "https" -Port 443 -HostHeader "test2.nielander.nl" -SslFlags 1
New-WebBinding -Name "ERH.HeatScans.Reporting" -Protocol "https" -Port 443 -HostHeader "test3.nielander.nl" -SslFlags 1

# Bind certificates
$cert2 = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test2.nielander.nl*" }
$binding2 = Get-WebBinding -Name "ERH.HeatScans.Reporting" -Protocol "https" -HostHeader "test2.nielander.nl"
$binding2.AddSslCertificate($cert2.Thumbprint, "My")
```

### Different Port

```powershell
.\Setup-IIS-CustomDomain.ps1 -DnsName "test.nielander.nl" -HttpsPort 44300
```

Browse to: `https://test.nielander.nl:44300/`

### Custom Physical Path

```powershell
.\Setup-IIS-CustomDomain.ps1 -DnsName "test.nielander.nl" -PhysicalPath "D:\Websites\HeatScans"
```

## Troubleshooting

### Browser Shows "Your connection is not private"
This is normal for self-signed certificates. Options:
1. Click "Advanced" ? "Proceed to test.nielander.nl"
2. Or trust the certificate (already done by script)
3. For Chrome: type `thisisunsafe` on the warning page

### Certificate Not Found in IIS
```powershell
# Verify certificate exists
Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }

# If found, manually bind it
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
$binding = Get-WebBinding -Name "ERH.HeatScans.Reporting" -Protocol "https" -HostHeader "test.nielander.nl"
$binding.AddSslCertificate($cert.Thumbprint, "My")
```

### "Unable to connect" Error
1. Check IIS site is running:
   ```powershell
   Get-Website -Name "ERH.HeatScans.Reporting"
   Start-Website -Name "ERH.HeatScans.Reporting"
   ```

2. Check hosts file has entry:
   ```
   127.0.0.1 test.nielander.nl
   ```

3. Verify binding exists:
   ```powershell
   Get-WebBinding -Name "ERH.HeatScans.Reporting" | Format-Table
   ```

### HTTP Error 503 - Service Unavailable
Application pool might be stopped:
```powershell
Start-WebAppPool -Name "ERH.HeatScans.Reporting.AppPool"
```

### Certificate Expired
Create a new certificate:
```powershell
.\Setup-Certificate-TestDomain.ps1
```
Choose "Y" when asked to create a new certificate.

## Production Considerations

### ?? Self-Signed Certificates Are Not for Production!

For production deployment of `test.nielander.nl`:

1. **Get a Real SSL Certificate**
   - Use Let's Encrypt (free): https://letsencrypt.org/
   - Or purchase from a CA (Sectigo, DigiCert, etc.)

2. **Let's Encrypt with IIS**
   ```powershell
   # Install win-acme (formerly letsencrypt-win-simple)
   # Download from: https://www.win-acme.com/
   
   wacs.exe --target iis --siteid 1 --host test.nielander.nl
   ```

3. **Manual Certificate Installation**
   - Get certificate files (.crt, .key, .ca-bundle)
   - Import to IIS:
     1. IIS Manager ? Server Certificates ? Complete Certificate Request
     2. Select the certificate
     3. Update site binding to use new certificate

4. **DNS Configuration**
   - Point `test.nielander.nl` to your server's public IP
   - Add A record in your DNS provider (where nielander.nl is hosted)
   - Wait for DNS propagation (can take up to 48 hours)

5. **Firewall Rules**
   - Open port 443 for HTTPS traffic
   - Configure Windows Firewall:
     ```powershell
     New-NetFirewallRule -DisplayName "HTTPS Inbound" -Direction Inbound -LocalPort 443 -Protocol TCP -Action Allow
     ```

## Testing Checklist

- [ ] Certificate created and trusted
- [ ] IIS binding configured
- [ ] Hosts file updated
- [ ] Application published to IIS
- [ ] Can browse to `https://test.nielander.nl`
- [ ] API endpoints responding correctly
- [ ] No certificate warnings (or warnings can be bypassed)
- [ ] Google authentication working
- [ ] Image uploads/downloads working

## Scripts Summary

| Script | Purpose | Usage |
|--------|---------|-------|
| `Setup-Certificate-TestDomain.ps1` | Create certificate only | For certificate management |
| `Setup-Certificate-TestDomain.bat` | Certificate wrapper | Double-click alternative |
| `Setup-IIS-CustomDomain.ps1` | Full IIS + certificate setup | Complete automated setup |
| `Setup-IIS.ps1` | Original setup (localhost) | For localhost development |

## Additional Resources

- [IIS Server Name Indication (SNI)](https://docs.microsoft.com/en-us/iis/get-started/whats-new-in-iis-8/iis-80-server-name-indication-sni-ssl-scalability)
- [Managing SSL Certificates in IIS](https://docs.microsoft.com/en-us/iis/manage/configuring-security/how-to-set-up-ssl-on-iis)
- [Self-Signed Certificates with PowerShell](https://docs.microsoft.com/en-us/powershell/module/pki/new-selfsignedcertificate)
- [Let's Encrypt Win-ACME](https://www.win-acme.com/)

## Support

For issues:
1. Check Event Viewer ? Windows Logs ? Application
2. Check IIS logs in `C:\inetpub\logs\LogFiles\`
3. Enable detailed errors in Web.config temporarily
4. Review the troubleshooting section above
