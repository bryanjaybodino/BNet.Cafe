#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$ProcessName = "BNet.Cafe.Websocket"
$ZipUrl = "https://github.com/bryanjaybodino/BNet.Cafe.Timer/archive/refs/heads/main.zip"
$TargetDir = $PSScriptRoot
$TempZip = "$env:TEMP\BNet_Repo.zip"
$TempExtract = "$env:TEMP\BNet_Extracted"

# SELF-RELOCATION PROTECTION (Prevents file locks on patcher files while updating)
$RunningScriptPath = $MyInvocation.MyCommand.Path
if ($RunningScriptPath -and -not $env:BNET_PATCHER_RUNNING_FROM_TEMP) {
    $RunnerCopy = "$env:TEMP\patcher_runner.ps1"
    Copy-Item -Path $RunningScriptPath -Destination $RunnerCopy -Force
    
    $BatSource = Join-Path $TargetDir "patcher.bat"
    if (Test-Path $BatSource) {
        Copy-Item -Path $BatSource -Destination "$env:TEMP\patcher.bat" -Force
    }

    $env:BNET_PATCHER_RUNNING_FROM_TEMP = "1"
    Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$RunnerCopy`"" -Verb RunAs
    exit
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     BNet.Cafe.Server Auto-Patcher        " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "[INFO] Target Directory: $TargetDir" -ForegroundColor Gray
Write-Host ""

# STEP 1: Terminate Process
Write-Host "[STEP 1/4] Checking for running process: $ProcessName.exe..." -ForegroundColor Yellow
$RunningProcess = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
if ($RunningProcess) {
    Write-Host "[INFO] Terminating $ProcessName..." -ForegroundColor DarkYellow
    Stop-Process -Name $ProcessName -Force
    Start-Sleep -Seconds 1
    Write-Host "[SUCCESS] Process terminated." -ForegroundColor Green
} else {
    Write-Host "[INFO] Process is not running." -ForegroundColor Gray
}
Write-Host ""

# STEP 2: Download Repository Archive
Write-Host "[STEP 2/4] Downloading latest update package..." -ForegroundColor Yellow
if (Test-Path $TempZip) { Remove-Item $TempZip -Force }
if (Test-Path $TempExtract) { Remove-Item $TempExtract -Recurse -Force }

try {
    Invoke-WebRequest -Uri $ZipUrl -OutFile $TempZip
    Write-Host "[SUCCESS] Download completed." -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Download failed: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# STEP 3: Extract and Update Server Folder
Write-Host "[STEP 3/4] Extracting and updating server files..." -ForegroundColor Yellow
Expand-Archive -Path $TempZip -DestinationPath $TempExtract -Force

$ServerSourceDir = Get-ChildItem -Path $TempExtract -Recurse -Directory | Where-Object { $_.Name -eq "BNet.Cafe.Server" } | Select-Object -First 1

if (-not $ServerSourceDir) {
    Write-Host "[ERROR] Could not find 'BNet.Cafe.Server' folder in the downloaded archive." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Copy all files safely into your repository directory
Get-ChildItem -Path $ServerSourceDir.FullName | ForEach-Object {
    Write-Host "  -> Updating: $($_.Name)" -ForegroundColor Cyan
    Copy-Item -Path $_.FullName -Destination $TargetDir -Recurse -Force
}
Write-Host "[SUCCESS] Files updated successfully right here." -ForegroundColor Green
Write-Host ""

# STEP 4: Cleanup
Write-Host "[STEP 4/4] Cleaning up temp files..." -ForegroundColor Yellow
if (Test-Path $TempZip) { Remove-Item $TempZip -Force }
if (Test-Path $TempExtract) { Remove-Item $TempExtract -Recurse -Force }
Write-Host "[SUCCESS] Cleanup finished." -ForegroundColor Green
Write-Host ""

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     UPDATE COMPLETE SUCCESSFULLY!        " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Auto-run the WebSocket application without prompting
$ExePath = Join-Path $TargetDir "$ProcessName.exe"
if (Test-Path $ExePath) {
    Write-Host "[INFO] Automatically starting $ProcessName.exe..." -ForegroundColor Yellow
    Start-Process $ExePath
} else {
    Write-Host "[WARNING] Executable not found to auto-launch." -ForegroundColor DarkYellow
    Start-Sleep -Seconds 3
}