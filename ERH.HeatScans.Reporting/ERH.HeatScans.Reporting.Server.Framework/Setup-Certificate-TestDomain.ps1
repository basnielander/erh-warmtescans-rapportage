# Create Self-Signed Certificate for test.nielander.nl
# Run this script as Administrator

param(
    [string]$DnsName = "test.nielander.nl",
    [string]$FriendlyName = "test.nielander.nl",
    [int]$ValidYears = 5
)

# Ensure script is run as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

Write-Host "=== Creating Self-Signed Certificate for $DnsName ===" -ForegroundColor Cyan

# 1. Create the Self-Signed Certificate
Write-Host "`n1. Creating certificate..." -ForegroundColor Yellow

try {
    # Check if certificate already exists
    $existingCert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { 
        $_.Subject -like "*$DnsName*" -or $_.FriendlyName -eq $FriendlyName 
    }

    if ($existingCert) {
        Write-Host "   Certificate already exists!" -ForegroundColor Yellow
        Write-Host "   Subject: $($existingCert.Subject)" -ForegroundColor White
        Write-Host "   Thumbprint: $($existingCert.Thumbprint)" -ForegroundColor White
        Write-Host "   Expires: $($existingCert.NotAfter)" -ForegroundColor White
        
        $response = Read-Host "`n   Do you want to create a new certificate? (Y/N)"
        if ($response -ne "Y" -and $response -ne "y") {
            Write-Host "`n   Using existing certificate." -ForegroundColor Green
            $cert = $existingCert
        } else {
            Write-Host "`n   Creating new certificate..." -ForegroundColor Yellow
            $cert = New-SelfSignedCertificate `
                -DnsName $DnsName `
                -CertStoreLocation "Cert:\LocalMachine\My" `
                -FriendlyName $FriendlyName `
                -NotAfter (Get-Date).AddYears($ValidYears) 
                #`
                #-KeyUsage DigitalSignature, KeyEncipherment `
                #-KeyAlgorithm RSA `
                #-KeyLength 2048 `
                #-HashAlgorithm SHA256
            
            Write-Host "   New certificate created!" -ForegroundColor Green
        }
    } else {
        $cert = New-SelfSignedCertificate `
            -DnsName $DnsName `
            -CertStoreLocation "Cert:\LocalMachine\My" `
            -FriendlyName $FriendlyName `
            -NotAfter (Get-Date).AddYears($ValidYears) 
            #`
            #-KeyUsage DigitalSignature, KeyEncipherment `
            #-KeyAlgorithm RSA `
            #-KeyLength 2048 `
            #-HashAlgorithm SHA256 `
            #-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1", "2.5.29.17={text}DNS=$DnsName&DNS=www.$DnsName")
        
        Write-Host "   Certificate created successfully!" -ForegroundColor Green
    }

    Write-Host "`n   Certificate Details:" -ForegroundColor White
    Write-Host "   -------------------" -ForegroundColor White
    Write-Host "   Subject: $($cert.Subject)" -ForegroundColor White
    Write-Host "   Issuer: $($cert.Issuer)" -ForegroundColor White
    Write-Host "   Thumbprint: $($cert.Thumbprint)" -ForegroundColor White
    Write-Host "   Valid From: $($cert.NotBefore)" -ForegroundColor White
    Write-Host "   Valid To: $($cert.NotAfter)" -ForegroundColor White
    Write-Host "   DNS Names: $($cert.DnsNameList.Unicode -join ', ')" -ForegroundColor White

} catch {
    Write-Error "Failed to create certificate: $_"
    exit 1
}

# 2. Trust the Certificate
Write-Host "`n2. Adding certificate to Trusted Root Certification Authorities..." -ForegroundColor Yellow

try {
    $rootStore = Get-Item -Path "Cert:\LocalMachine\Root"
    $existingRootCert = Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { 
        $_.Thumbprint -eq $cert.Thumbprint 
    }

    if ($existingRootCert) {
        Write-Host "   Certificate already trusted." -ForegroundColor Green
    } else {
        $rootStore.Open("ReadWrite")
        $rootStore.Add($cert)
        $rootStore.Close()
        Write-Host "   Certificate added to Trusted Root store." -ForegroundColor Green
    }
} catch {
    Write-Error "Failed to trust certificate: $_"
    exit 1
}

# 3. Export Certificate (Optional)
Write-Host "`n3. Export certificate (optional)..." -ForegroundColor Yellow
$exportPath = Join-Path $PSScriptRoot "certificates"

$response = Read-Host "   Do you want to export the certificate? (Y/N)"
if ($response -eq "Y" -or $response -eq "y") {
    if (-not (Test-Path $exportPath)) {
        New-Item -Path $exportPath -ItemType Directory -Force | Out-Null
    }

    # Export certificate (public key only)
    $certFile = Join-Path $exportPath "$DnsName.cer"
    Export-Certificate -Cert $cert -FilePath $certFile -Type CERT | Out-Null
    Write-Host "   Certificate exported to: $certFile" -ForegroundColor Green

    # Export with private key (password protected)
    $pfxFile = Join-Path $exportPath "$DnsName.pfx"
    $password = Read-Host "   Enter password for PFX file (or leave empty to skip)" -AsSecureString
    
    if ($password.Length -gt 0) {
        Export-PfxCertificate -Cert $cert -FilePath $pfxFile -Password $password | Out-Null
        Write-Host "   Certificate with private key exported to: $pfxFile" -ForegroundColor Green
        Write-Host "   WARNING: Keep this file secure!" -ForegroundColor Red
    }
}

# 4. Configure IIS Binding (Optional)
Write-Host "`n4. Configure IIS binding (optional)..." -ForegroundColor Yellow
$response = Read-Host "   Do you want to add this certificate to IIS now? (Y/N)"

if ($response -eq "Y" -or $response -eq "y") {
    try {
        Import-Module WebAdministration -ErrorAction Stop
        
        $siteName = Read-Host "   Enter IIS site name (default: ERH.HeatScans.Reporting)"
        if ([string]::IsNullOrWhiteSpace($siteName)) {
            $siteName = "ERH.HeatScans.Reporting"
        }

        $site = Get-Website -Name $siteName -ErrorAction SilentlyContinue
        
        if ($site) {
            # Remove existing HTTPS binding if it exists
            $existingBinding = Get-WebBinding -Name $siteName -Protocol "https" -HostHeader $DnsName -ErrorAction SilentlyContinue
            if ($existingBinding) {
                Write-Host "   Removing existing HTTPS binding..." -ForegroundColor Yellow
                Remove-WebBinding -Name $siteName -Protocol "https" -HostHeader $DnsName
            }

            # Add new HTTPS binding
            $port = Read-Host "   Enter HTTPS port (default: 443)"
            if ([string]::IsNullOrWhiteSpace($port)) {
                $port = 443
            }

            New-WebBinding -Name $siteName `
                -Protocol "https" `
                -Port $port `
                -HostHeader $DnsName `
                -SslFlags 1

            # Bind certificate
            $binding = Get-WebBinding -Name $siteName -Protocol "https" -HostHeader $DnsName
            $binding.AddSslCertificate($cert.Thumbprint, "My")

            Write-Host "   IIS binding configured successfully!" -ForegroundColor Green
            Write-Host "   Site: $siteName" -ForegroundColor White
            Write-Host "   URL: https://${DnsName}:${port}/" -ForegroundColor White
        } else {
            Write-Host "   WARNING: Site '$siteName' not found in IIS." -ForegroundColor Yellow
            Write-Host "   You can manually bind the certificate in IIS Manager." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   WARNING: Could not configure IIS: $_" -ForegroundColor Yellow
        Write-Host "   You can manually bind the certificate in IIS Manager." -ForegroundColor Yellow
    }
}

# 5. Update hosts file
Write-Host "`n5. Update hosts file..." -ForegroundColor Yellow
$hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
$hostsContent = Get-Content $hostsPath -Raw

if ($hostsContent -notmatch [regex]::Escape($DnsName)) {
    $response = Read-Host "   Add '$DnsName' to hosts file pointing to 127.0.0.1? (Y/N)"
    
    if ($response -eq "Y" -or $response -eq "y") {
        Add-Content -Path $hostsPath -Value "`n127.0.0.1 $DnsName"
        Write-Host "   Added to hosts file." -ForegroundColor Green
    }
} else {
    Write-Host "   '$DnsName' already exists in hosts file." -ForegroundColor Green
}

# Summary
Write-Host "`n=== Certificate Setup Complete ===" -ForegroundColor Cyan
Write-Host "`nCertificate Information:" -ForegroundColor White
Write-Host "  Domain: $DnsName" -ForegroundColor White
Write-Host "  Friendly Name: $FriendlyName" -ForegroundColor White
Write-Host "  Thumbprint: $($cert.Thumbprint)" -ForegroundColor White
Write-Host "  Store Location: Cert:\LocalMachine\My" -ForegroundColor White
Write-Host "  Valid Until: $($cert.NotAfter)" -ForegroundColor White

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. In IIS Manager, select your site" -ForegroundColor White
Write-Host "2. Click 'Bindings' in the Actions panel" -ForegroundColor White
Write-Host "3. Add/Edit HTTPS binding:" -ForegroundColor White
Write-Host "   - Type: https" -ForegroundColor White
Write-Host "   - IP: All Unassigned" -ForegroundColor White
Write-Host "   - Port: 443" -ForegroundColor White
Write-Host "   - Host name: $DnsName" -ForegroundColor White
Write-Host "   - SSL certificate: $FriendlyName" -ForegroundColor White
Write-Host "4. Add '$DnsName' to your hosts file if not done already:" -ForegroundColor White
Write-Host "   127.0.0.1 $DnsName" -ForegroundColor Gray
Write-Host "5. Browse to https://$DnsName" -ForegroundColor White

Write-Host "`nManual IIS Binding (if needed):" -ForegroundColor Cyan
Write-Host "New-WebBinding -Name 'ERH.HeatScans.Reporting' -Protocol https -Port 443 -HostHeader '$DnsName' -SslFlags 1" -ForegroundColor Gray
Write-Host "`$binding = Get-WebBinding -Name 'ERH.HeatScans.Reporting' -Protocol https -HostHeader '$DnsName'" -ForegroundColor Gray
Write-Host "`$binding.AddSslCertificate('$($cert.Thumbprint)', 'My')" -ForegroundColor Gray

Write-Host ""
