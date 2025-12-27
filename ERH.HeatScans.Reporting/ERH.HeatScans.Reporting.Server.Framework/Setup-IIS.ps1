# IIS Setup and Deployment Script for ERH.HeatScans.Reporting
# Run this script as Administrator

param(
    [string]$SiteName = "ERH.HeatScans.Reporting",
    [string]$AppPoolName = "ERH.HeatScans.Reporting.AppPool",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\ERH.HeatScans.Reporting",
    [string]$HostName = "localhost",
    [int]$HttpsPort = 443,
    [string]$CertificateFriendlyName = "ERH.HeatScans.LocalDev"
)

# Ensure script is run as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

Write-Host "=== IIS Setup for ERH.HeatScans.Reporting ===" -ForegroundColor Cyan

# Import WebAdministration module
Import-Module WebAdministration -ErrorAction Stop

# 1. Create Self-Signed Certificate
Write-Host "`n1. Creating Self-Signed Certificate..." -ForegroundColor Yellow
$existingCert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.FriendlyName -eq $CertificateFriendlyName }

if ($existingCert) {
    Write-Host "   Certificate '$CertificateFriendlyName' already exists." -ForegroundColor Green
    $cert = $existingCert
} else {
    $cert = New-SelfSignedCertificate `
        -DnsName $HostName `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -FriendlyName $CertificateFriendlyName `
        -NotAfter (Get-Date).AddYears(5) `
        -KeyUsage DigitalSignature, KeyEncipherment `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
    
    Write-Host "   Certificate created with thumbprint: $($cert.Thumbprint)" -ForegroundColor Green
}

# 2. Trust the Certificate
Write-Host "`n2. Trusting the Certificate..." -ForegroundColor Yellow
$rootStore = Get-Item -Path "Cert:\LocalMachine\Root"
$existingRootCert = Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Thumbprint -eq $cert.Thumbprint }

if (-not $existingRootCert) {
    $rootStore.Open("ReadWrite")
    $rootStore.Add($cert)
    $rootStore.Close()
    Write-Host "   Certificate added to Trusted Root." -ForegroundColor Green
} else {
    Write-Host "   Certificate already in Trusted Root." -ForegroundColor Green
}

# 3. Create Application Pool
Write-Host "`n3. Creating Application Pool..." -ForegroundColor Yellow
$appPool = Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue

if ($appPool) {
    Write-Host "   Application Pool '$AppPoolName' already exists. Stopping..." -ForegroundColor Yellow
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
} else {
    New-WebAppPool -Name $AppPoolName
    Write-Host "   Application Pool '$AppPoolName' created." -ForegroundColor Green
}

# Configure Application Pool
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value "v4.0"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedPipelineMode" -Value "Integrated"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value ([TimeSpan]::FromMinutes(0))
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value ([TimeSpan]::FromMinutes(0))
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false

Write-Host "   Application Pool configured for .NET 4.8, x64, AlwaysRunning" -ForegroundColor Green

# 4. Create Physical Directory
Write-Host "`n4. Creating Physical Directory..." -ForegroundColor Yellow
if (-not (Test-Path $PhysicalPath)) {
    New-Item -Path $PhysicalPath -ItemType Directory -Force | Out-Null
    Write-Host "   Directory created: $PhysicalPath" -ForegroundColor Green
} else {
    Write-Host "   Directory already exists: $PhysicalPath" -ForegroundColor Green
}

# 5. Set Permissions
Write-Host "`n5. Setting Permissions..." -ForegroundColor Yellow
$acl = Get-Acl $PhysicalPath
$identities = @("IIS_IUSRS", "IUSR", "IIS APPPOOL\$AppPoolName")

foreach ($identity in $identities) {
    try {
        $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            $identity,
            "ReadAndExecute",
            "ContainerInherit,ObjectInherit",
            "None",
            "Allow"
        )
        $acl.SetAccessRule($rule)
        Write-Host "   Permission granted to $identity" -ForegroundColor Green
    } catch {
        Write-Host "   Warning: Could not set permission for $identity" -ForegroundColor Yellow
    }
}
Set-Acl $PhysicalPath $acl

# 6. Create or Update Website
Write-Host "`n6. Creating/Updating Website..." -ForegroundColor Yellow
$site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue

if ($site) {
    Write-Host "   Website '$SiteName' already exists. Updating..." -ForegroundColor Yellow
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Remove-Website -Name $SiteName
}

# Create new website with HTTPS binding
New-Website -Name $SiteName `
    -PhysicalPath $PhysicalPath `
    -ApplicationPool $AppPoolName `
    -Port $HttpsPort `
    -Ssl `
    -HostHeader $HostName `
    -Force

Write-Host "   Website '$SiteName' created." -ForegroundColor Green

# 7. Bind Certificate
Write-Host "`n7. Binding SSL Certificate..." -ForegroundColor Yellow
$binding = Get-WebBinding -Name $SiteName -Protocol "https"

if ($binding) {
    $binding.AddSslCertificate($cert.Thumbprint, "My")
    Write-Host "   SSL Certificate bound successfully." -ForegroundColor Green
} else {
    Write-Host "   Warning: Could not find HTTPS binding." -ForegroundColor Yellow
}

# 8. Start Application Pool and Website
Write-Host "`n8. Starting Application Pool and Website..." -ForegroundColor Yellow
Start-WebAppPool -Name $AppPoolName
Start-Website -Name $SiteName
Write-Host "   Application Pool and Website started." -ForegroundColor Green

# 9. Update hosts file (if needed)
if ($HostName -ne "localhost" -and $HostName -ne "") {
    Write-Host "`n9. Updating hosts file..." -ForegroundColor Yellow
    $hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
    $hostsContent = Get-Content $hostsPath
    $hostEntry = "127.0.0.1 $HostName"
    
    if ($hostsContent -notcontains $hostEntry) {
        Add-Content -Path $hostsPath -Value "`n$hostEntry"
        Write-Host "   Added '$hostEntry' to hosts file." -ForegroundColor Green
    } else {
        Write-Host "   Host entry already exists in hosts file." -ForegroundColor Green
    }
}

# 10. Display Summary
Write-Host "`n=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "Site Name: $SiteName" -ForegroundColor White
Write-Host "Application Pool: $AppPoolName" -ForegroundColor White
Write-Host "Physical Path: $PhysicalPath" -ForegroundColor White
Write-Host "URL: https://${HostName}:${HttpsPort}/" -ForegroundColor White
Write-Host "Certificate Thumbprint: $($cert.Thumbprint)" -ForegroundColor White
Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Publish your application using Visual Studio (right-click project -> Publish)" -ForegroundColor White
Write-Host "2. Select the 'LocalIIS' publish profile" -ForegroundColor White
Write-Host "3. Copy google-credentials.json to $PhysicalPath" -ForegroundColor White
Write-Host "4. Browse to https://${HostName}:${HttpsPort}/api/folders-and-files/structure" -ForegroundColor White
Write-Host ""
