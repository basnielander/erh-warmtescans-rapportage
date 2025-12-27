# Quick Reference - test.nielander.nl Certificate Setup

## ?? Quick Start (One Command)

```powershell
# As Administrator - Full setup (certificate + IIS)
.\Setup-IIS-CustomDomain.ps1 -DnsName "test.nielander.nl"
```

## ?? What You Need

- Windows with IIS installed
- Administrator privileges
- PowerShell

## ?? Setup Options

### Option 1: Certificate Only
```powershell
.\Setup-Certificate-TestDomain.ps1
```
Or double-click: `Setup-Certificate-TestDomain.bat` (Run as Admin)

### Option 2: Full IIS Setup
```powershell
.\Setup-IIS-CustomDomain.ps1 -DnsName "test.nielander.nl"
```

## ?? Certificate Details

- **Domain**: test.nielander.nl (+ www.test.nielander.nl)
- **Friendly Name**: ERH.HeatScans.TestDomain
- **Type**: Self-signed (RSA 2048-bit, SHA256)
- **Location**: Cert:\LocalMachine\My
- **Trusted**: Yes (added to Trusted Root)
- **Valid**: 5 years

## ?? URLs

- **Local**: https://test.nielander.nl
- **API Structure**: https://test.nielander.nl/api/folders-and-files/structure
- **API Files**: https://test.nielander.nl/api/folders-and-files/files
- **API Image**: https://test.nielander.nl/api/folders-and-files/image?fileId={id}

## ? Verification

### Check Certificate
```powershell
Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
```

### Check IIS Binding
```powershell
Get-WebBinding -Name "ERH.HeatScans.Reporting" | Format-Table
```

### Check Website Status
```powershell
Get-Website -Name "ERH.HeatScans.Reporting"
```

### Check Hosts File
```powershell
Get-Content C:\Windows\System32\drivers\etc\hosts | Select-String "test.nielander.nl"
```

## ?? Troubleshooting

### Site Won't Load
```powershell
Start-WebAppPool -Name "ERH.HeatScans.Reporting.AppPool"
Start-Website -Name "ERH.HeatScans.Reporting"
```

### Browser Certificate Warning
Normal for self-signed certs. Click "Advanced" ? "Proceed"

### Certificate Not Found
```powershell
.\Setup-Certificate-TestDomain.ps1
```

## ?? Update http-client.env.json

```json
{
  "dev": {
    "BaseUrl": "https://test.nielander.nl",
    "AccessToken": "your-token",
    "AddressFolderId": "your-folder-id"
  }
}
```

## ?? Certificate Management

### Export (with private key)
```powershell
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" }
$pwd = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "test.nielander.nl.pfx" -Password $pwd
```

### Remove Certificate
```powershell
Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*test.nielander.nl*" } | Remove-Item
Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like "*test.nielander.nl*" } | Remove-Item
```

## ?? Documentation Files

- `CUSTOM-DOMAIN-SETUP.md` - Complete guide
- `IIS-DEPLOYMENT.md` - General IIS deployment
- `IIS-SETUP-QUICKSTART.md` - Localhost setup

## ?? Production Warning

**Self-signed certificates are for development/testing only!**

For production:
1. Use Let's Encrypt (free) or purchase real SSL certificate
2. Configure proper DNS records
3. Replace self-signed certificate

## ?? Next Steps After Setup

1. [ ] Run setup script
2. [ ] Publish application (Visual Studio ? Publish ? LocalIIS)
3. [ ] Copy google-credentials.json to C:\inetpub\wwwroot\ERH.HeatScans.Reporting\
4. [ ] Test: https://test.nielander.nl/api/folders-and-files/structure
5. [ ] Update HTTP client environment variables
6. [ ] Test API endpoints

## ?? Common Commands

```powershell
# Restart IIS
iisreset

# Restart App Pool
Restart-WebAppPool -Name "ERH.HeatScans.Reporting.AppPool"

# Restart Website
Restart-WebItem "IIS:\Sites\ERH.HeatScans.Reporting"

# View IIS Logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC*\*.log" -Tail 50

# Check Event Logs
Get-EventLog -LogName Application -Source "ASP.NET*" -Newest 10
```

## ?? Useful Links

- IIS Manager: Run `inetmgr`
- Certificate Manager: Run `certmgr.msc`
- Hosts File: `C:\Windows\System32\drivers\etc\hosts`
- IIS Logs: `C:\inetpub\logs\LogFiles\`

---

**Support**: See CUSTOM-DOMAIN-SETUP.md for detailed troubleshooting
