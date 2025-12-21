# Setup IIS Express SSL Certificate for Port 7209
# This script creates and registers a self-signed certificate for HTTPS development

param(
    [int]$Port = 7209,
    [string]$HostName = "localhost"
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "IIS Express SSL Certificate Setup" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "? ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please:" -ForegroundColor Yellow
    Write-Host "1. Right-click PowerShell" -ForegroundColor White
    Write-Host "2. Select 'Run as Administrator'" -ForegroundColor White
    Write-Host "3. Run this script again" -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "? Running with Administrator privileges" -ForegroundColor Green
Write-Host ""

# Step 1: Check if certificate already exists for this port
Write-Host "Step 1: Checking existing SSL certificate bindings..." -ForegroundColor Cyan
$existingBinding = netsh http show sslcert | Select-String "0.0.0.0:$Port"
if ($existingBinding) {
    Write-Host "? SSL certificate already bound to port $Port" -ForegroundColor Yellow
    Write-Host ""
    $remove = Read-Host "Remove existing binding? (Y/N)"
    if ($remove -eq "Y") {
        Write-Host "Removing existing binding..." -ForegroundColor Yellow
        netsh http delete sslcert ipport=0.0.0.0:$Port
        Write-Host "? Existing binding removed" -ForegroundColor Green
    } else {
        Write-Host "Keeping existing binding. Exiting." -ForegroundColor Yellow
        Read-Host "Press Enter to exit"
        exit 0
    }
}
Write-Host ""

# Step 2: Find or create IIS Express certificate
Write-Host "Step 2: Finding IIS Express Development Certificate..." -ForegroundColor Cyan

# Look for existing IIS Express certificate
$iisExpressCert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { 
    $_.Subject -match "CN=localhost" -and 
    $_.Issuer -match "CN=localhost" -and
    $_.NotAfter -gt (Get-Date)
} | Select-Object -First 1

if (-not $iisExpressCert) {
    Write-Host "? IIS Express certificate not found. Creating new self-signed certificate..." -ForegroundColor Yellow
    
    # Create a new self-signed certificate
    $cert = New-SelfSignedCertificate `
        -DnsName $HostName `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -FriendlyName "IIS Express Development Certificate" `
        -NotAfter (Get-Date).AddYears(10) `
        -KeyUsage DigitalSignature, KeyEncipherment `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
        -KeyExportPolicy Exportable
    
    Write-Host "? Created new self-signed certificate" -ForegroundColor Green
    Write-Host "  Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
    
    # Export to Trusted Root to avoid browser warnings
    Write-Host "  Adding certificate to Trusted Root Certification Authorities..." -ForegroundColor Gray
    $certPath = "Cert:\LocalMachine\My\$($cert.Thumbprint)"
    $certToExport = Get-Item $certPath
    Export-Certificate -Cert $certToExport -FilePath "$env:TEMP\localhost.cer" -Force | Out-Null
    Import-Certificate -FilePath "$env:TEMP\localhost.cer" -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
    Remove-Item "$env:TEMP\localhost.cer" -Force
    Write-Host "  ? Certificate trusted" -ForegroundColor Green
    
    $iisExpressCert = $cert
} else {
    Write-Host "? Found existing IIS Express certificate" -ForegroundColor Green
    Write-Host "  Thumbprint: $($iisExpressCert.Thumbprint)" -ForegroundColor Gray
    Write-Host "  Subject: $($iisExpressCert.Subject)" -ForegroundColor Gray
    Write-Host "  Valid until: $($iisExpressCert.NotAfter)" -ForegroundColor Gray
    
    # Check if it's in Trusted Root
    $trustedCert = Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Thumbprint -eq $iisExpressCert.Thumbprint }
    if (-not $trustedCert) {
        Write-Host "  ? Certificate not in Trusted Root. Adding..." -ForegroundColor Yellow
        $certPath = "Cert:\LocalMachine\My\$($iisExpressCert.Thumbprint)"
        $certToExport = Get-Item $certPath
        Export-Certificate -Cert $certToExport -FilePath "$env:TEMP\localhost.cer" -Force | Out-Null
        Import-Certificate -FilePath "$env:TEMP\localhost.cer" -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
        Remove-Item "$env:TEMP\localhost.cer" -Force
        Write-Host "  ? Certificate now trusted" -ForegroundColor Green
    }
}
Write-Host ""

# Step 3: Get the application ID (IIS Express uses a specific GUID)
Write-Host "Step 3: Configuring SSL binding..." -ForegroundColor Cyan
$appId = "{214124cd-d05b-4309-9af9-9caa44b2b74a}" # Standard IIS Express GUID
Write-Host "  Using IIS Express Application ID: $appId" -ForegroundColor Gray

# Step 4: Bind certificate to port
Write-Host "  Binding certificate to port $Port..." -ForegroundColor Gray
$certHash = $iisExpressCert.Thumbprint

# Add SSL certificate binding
$result = netsh http add sslcert ipport=0.0.0.0:$Port certhash=$certHash appid=$appId

if ($LASTEXITCODE -eq 0) {
    Write-Host "? SSL certificate successfully bound to port $Port" -ForegroundColor Green
} else {
    Write-Host "? Failed to bind SSL certificate" -ForegroundColor Red
    Write-Host "Error output:" -ForegroundColor Yellow
    Write-Host $result
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Step 5: Verify the binding
Write-Host "Step 4: Verifying SSL certificate binding..." -ForegroundColor Cyan
$verification = netsh http show sslcert ipport=0.0.0.0:$Port
if ($verification) {
    Write-Host "? SSL certificate binding verified" -ForegroundColor Green
    Write-Host ""
    Write-Host $verification
} else {
    Write-Host "? Could not verify binding" -ForegroundColor Yellow
}
Write-Host ""

# Step 6: Configure Windows Firewall
Write-Host "Step 5: Checking Windows Firewall..." -ForegroundColor Cyan
$firewallRule = Get-NetFirewallRule -DisplayName "IIS Express - Port $Port" -ErrorAction SilentlyContinue
if (-not $firewallRule) {
    Write-Host "  Creating firewall rule..." -ForegroundColor Gray
    New-NetFirewallRule `
        -DisplayName "IIS Express - Port $Port" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort $Port `
        -Action Allow `
        -Profile Any | Out-Null
    Write-Host "? Firewall rule created" -ForegroundColor Green
} else {
    Write-Host "? Firewall rule already exists" -ForegroundColor Green
}
Write-Host ""

# Summary
Write-Host "=========================================" -ForegroundColor Green
Write-Host "? Setup Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Certificate Details:" -ForegroundColor Cyan
Write-Host "  Thumbprint: $($iisExpressCert.Thumbprint)" -ForegroundColor White
Write-Host "  Subject: $($iisExpressCert.Subject)" -ForegroundColor White
Write-Host "  Issuer: $($iisExpressCert.Issuer)" -ForegroundColor White
Write-Host "  Valid From: $($iisExpressCert.NotBefore)" -ForegroundColor White
Write-Host "  Valid To: $($iisExpressCert.NotAfter)" -ForegroundColor White
Write-Host ""
Write-Host "Binding Details:" -ForegroundColor Cyan
Write-Host "  IP Address: 0.0.0.0" -ForegroundColor White
Write-Host "  Port: $Port" -ForegroundColor White
Write-Host "  URL: https://${HostName}:$Port" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Open Visual Studio" -ForegroundColor White
Write-Host "2. Open your solution" -ForegroundColor White
Write-Host "3. Press F5 to start debugging" -ForegroundColor White
Write-Host "4. Your application should now run at: https://${HostName}:$Port" -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: If you still see certificate warnings in your browser:" -ForegroundColor Gray
Write-Host "  - Close all browser windows and restart" -ForegroundColor Gray
Write-Host "  - Clear browser cache and SSL state" -ForegroundColor Gray
Write-Host "  - In Chrome: chrome://restart" -ForegroundColor Gray
Write-Host ""

Read-Host "Press Enter to exit"
