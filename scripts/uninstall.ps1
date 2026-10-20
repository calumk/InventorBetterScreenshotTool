# Developer cleanup only. If installed via Setup.exe, use Windows Installed Apps
# so Inno Setup can also remove its uninstall registration.
param([ValidateSet('2025')][string]$InventorYear = '2025')
$ErrorActionPreference = 'Stop'
if (Get-Process -Name Inventor -ErrorAction SilentlyContinue) {
    throw 'Close Autodesk Inventor before uninstalling the add-in.'
}
$destination = Join-Path $env:APPDATA "Autodesk\Inventor $InventorYear\Addins\InventorScreenshotTool"
if (Test-Path $destination) {
    Remove-Item -Path $destination -Recurse -Force
    Write-Host "Removed add-in files from: $destination"
} else {
    Write-Host 'Better Screenshot Tool is not installed in the developer location.'
}
Write-Host 'Screenshots and user preferences were not deleted.'
