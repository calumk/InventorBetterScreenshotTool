# Developer install/update. In GitHub Releases, end users get a Setup.exe instead.
param(
    [ValidateSet('2025')][string]$InventorYear = '2025',
    [string]$InventorBin = "C:\Program Files\Autodesk\Inventor $InventorYear\Bin"
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (Get-Process -Name Inventor -ErrorAction SilentlyContinue) {
    throw 'Please close Autodesk Inventor before installing or updating.'
}
& (Join-Path $PSScriptRoot 'package.ps1') -InventorBin $InventorBin
if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Build failed.' }
$source = Join-Path $PSScriptRoot '..\artifacts\plugin'
$destination = Join-Path $env:APPDATA "Autodesk\Inventor $InventorYear\Addins\InventorScreenshotTool"
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Get-ChildItem $source -File | Copy-Item -Destination $destination -Force
Write-Host "Updated Inventor $InventorYear add-in at $destination"
Write-Host 'Your settings.json and screenshot folders were not touched.'
Write-Host 'Restart Inventor to load the new version.'
