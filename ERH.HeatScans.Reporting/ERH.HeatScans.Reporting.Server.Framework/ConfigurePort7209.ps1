# Configure HTTPS Port 7209 for ERH.HeatScans.Reporting.Server.Framework
# This script updates the project to use HTTPS on port 7209

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Configure HTTPS Port 7209" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator for SSL certificate configuration
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "⚠ WARNING: Not running as Administrator" -ForegroundColor Yellow
    Write-Host "SSL certificate binding will not be configured automatically." -ForegroundColor Yellow
    Write-Host "You will need to run Setup-IISExpressSSL.ps1 as Administrator separately." -ForegroundColor Yellow
    Write-Host ""
}

$projectFile = ".\ERH.HeatScans.Reporting.Server.Framework.csproj"
$webConfig = ".\Web.config"

# Check if Visual Studio is running
$vsProcesses = Get-Process | Where-Object { $_.ProcessName -like "*devenv*" -or $_.ProcessName -like "*MSBuild*" }
if ($vsProcesses) {
    Write-Host "? WARNING: Visual Studio or MSBuild appears to be running" -ForegroundColor Yellow
    Write-Host "Please close Visual Studio before running this script" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Continue anyway? (Y/N)"
    if ($continue -ne "Y") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Step 1: Updating project file..." -ForegroundColor Cyan

# Backup project file
if (Test-Path $projectFile) {
    $backupFile = "$projectFile.backup-" + (Get-Date -Format "yyyyMMdd-HHmmss")
    Copy-Item $projectFile $backupFile
    Write-Host "? Created backup: $backupFile" -ForegroundColor Green
    
    # Read and update project file
    $content = Get-Content $projectFile -Raw
    
    # Update IISExpressSSLPort
    $content = $content -replace '<IISExpressSSLPort>\d+</IISExpressSSLPort>', '<IISExpressSSLPort>7209</IISExpressSSLPort>'
    
    # Save updated content
    Set-Content -Path $projectFile -Value $content -NoNewline
    Write-Host "? Updated IISExpressSSLPort to 7209" -ForegroundColor Green
} else {
    Write-Host "? Project file not found at $projectFile" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Checking .vs folder for IIS Express configuration..." -ForegroundColor Cyan

$vsFolder = ".\.vs"
$configFolder = "$vsFolder\config"

if (Test-Path $vsFolder) {
    Write-Host "Found .vs folder" -ForegroundColor Green
    
    # Find applicationhost.config
    $appHostConfig = Get-ChildItem -Path $vsFolder -Filter "applicationhost.config" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($appHostConfig) {
        Write-Host "Found applicationhost.config at: $($appHostConfig.FullName)" -ForegroundColor Green
        
        # Backup
        $backupAppHost = "$($appHostConfig.FullName).backup-" + (Get-Date -Format "yyyyMMdd-HHmmss")
        Copy-Item $appHostConfig.FullName $backupAppHost
        Write-Host "? Created backup: $backupAppHost" -ForegroundColor Green
        
        # Update applicationhost.config
        $appHostContent = Get-Content $appHostConfig.FullName -Raw
        
        # Update binding for port 7209 (HTTPS)
        if ($appHostContent -match 'bindingInformation="\*:7209:localhost"') {
            Write-Host "? Port 7209 binding already exists" -ForegroundColor Green
        } else {
            Write-Host "? Port 7209 binding not found in applicationhost.config" -ForegroundColor Yellow
            Write-Host "  IIS Express will create it on first run" -ForegroundColor Gray
        }
    } else {
        Write-Host "? applicationhost.config not found (will be created on first run)" -ForegroundColor Gray
    }
} else {
    Write-Host "? .vs folder not found (will be created on first run)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Step 3: Verifying SSL certificate for localhost..." -ForegroundColor Cyan

# Check if IIS Express development certificate exists
$certCheck = netsh http show sslcert 2>$null | Select-String "0.0.0.0:7209"
if ($certCheck) {
    Write-Host "✓ SSL certificate already registered for port 7209" -ForegroundColor Green
} else {
    Write-Host "❌ SSL certificate NOT registered for port 7209" -ForegroundColor Red
    Write-Host ""
    
    if ($isAdmin) {
        Write-Host "Attempting to configure SSL certificate..." -ForegroundColor Yellow
        
        # Find IIS Express certificate
        $iisExpressCert = Get-ChildItem -Path Cert:\LocalMachine\My -ErrorAction SilentlyContinue | Where-Object { 
            $_.Subject -match "CN=localhost" -and 
            $_.Issuer -match "CN=localhost" -and
            $_.NotAfter -gt (Get-Date)
        } | Select-Object -First 1
        
        if ($iisExpressCert) {
            Write-Host "Found IIS Express certificate: $($iisExpressCert.Thumbprint)" -ForegroundColor Green
            Write-Host "Binding certificate to port 7209..." -ForegroundColor Gray
            
            $appId = "{214124cd-d05b-4309-9af9-9caa44b2b74a}"
            $certHash = $iisExpressCert.Thumbprint
            
            $bindResult = netsh http add sslcert ipport=0.0.0.0:7209 certhash=$certHash appid=$appId 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ SSL certificate successfully bound to port 7209" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to bind certificate" -ForegroundColor Red
                Write-Host "Error: $bindResult" -ForegroundColor Yellow
            }
        } else {
            Write-Host "❌ IIS Express certificate not found" -ForegroundColor Red
            Write-Host ""
            Write-Host "Please run the following command as Administrator:" -ForegroundColor Yellow
            Write-Host "  .\Setup-IISExpressSSL.ps1" -ForegroundColor White
            Write-Host ""
            Write-Host "This will:" -ForegroundColor Gray
            Write-Host "  1. Create a self-signed certificate for localhost" -ForegroundColor Gray
            Write-Host "  2. Add it to Trusted Root Certification Authorities" -ForegroundColor Gray
            Write-Host "  3. Bind it to port 7209" -ForegroundColor Gray
            Write-Host "  4. Configure Windows Firewall" -ForegroundColor Gray
        }
    } else {
        Write-Host "To fix this, run as Administrator:" -ForegroundColor Yellow
        Write-Host "  .\Setup-IISExpressSSL.ps1" -ForegroundColor White
        Write-Host ""
        Write-Host "This script will:" -ForegroundColor Gray
        Write-Host "  1. Create a self-signed certificate for localhost" -ForegroundColor Gray
        Write-Host "  2. Add it to Trusted Root Certification Authorities" -ForegroundColor Gray
        Write-Host "  3. Bind it to port 7209" -ForegroundColor Gray
        Write-Host "  4. Configure Windows Firewall" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "? Configuration Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Project configured for HTTPS on port 7209" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Open Visual Studio" -ForegroundColor White
Write-Host "2. Open the solution" -ForegroundColor White
Write-Host "3. Press F5 to start debugging" -ForegroundColor White
Write-Host "4. The application will run at: https://localhost:7209/" -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: On first run, you may be prompted to:" -ForegroundColor Gray
Write-Host "  - Trust the IIS Express SSL certificate" -ForegroundColor Gray
Write-Host "  - Allow IIS Express through the firewall" -ForegroundColor Gray
Write-Host ""

# Also update the Angular client service if needed
Write-Host "Remember to update the Angular client baseUrl to:" -ForegroundColor Cyan
Write-Host "  private baseUrl = 'https://localhost:7209/api/user/googledrive';" -ForegroundColor Yellow
Write-Host ""

cd ERH.HeatScans.Reporting.Server.Framework
.\Setup-IISExpressSSL.ps1

Read-Host "Press Enter to exit"
