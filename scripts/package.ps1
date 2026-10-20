# Build and stage ONLY our own plugin binaries for deployment.
# Do NOT distribute Autodesk.Inventor.Interop.dll with this project.
param(
    [string]$InventorBin = 'C:\Program Files\Autodesk\Inventor 2025\Bin'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $root 'src\InventorScreenshotTool\InventorScreenshotTool.csproj'
$manifest = Join-Path $root 'src\InventorScreenshotTool\InventorScreenshotTool.addin'
$ico = Join-Path $root 'src\InventorScreenshotTool\Assets\BetterScreenshot.ico'
$interop = Join-Path $InventorBin 'Autodesk.Inventor.Interop.dll'
if (-not (Test-Path $interop)) {
    throw "Missing Inventor 2025 Interop DLL at '$interop'. Run on a Windows machine with Inventor 2025 installed."
}
Write-Host "Building Inventor Screenshot Tool using $interop"
& dotnet build $project --configuration Release "-p:InventorBin=$InventorBin" '-p:ContinuousIntegrationBuild=true'
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed. No package produced.' }

$output = Join-Path $root 'src\InventorScreenshotTool\bin\Release\net8.0-windows'
$dll = Join-Path $output 'InventorScreenshotTool.dll'
if (-not (Test-Path $dll)) { throw "Build output missing: $dll" }
$stage = Join-Path $root 'artifacts\plugin'
if (Test-Path $stage) { Remove-Item $stage -Force -Recurse }
New-Item -ItemType Directory -Path $stage -Force | Out-Null
Copy-Item $dll $stage
Copy-Item $manifest $stage
Copy-Item $ico (Join-Path $stage 'BetterScreenshot.ico')
# Optional metadata, not required by Inventor but useful when present.
$deps = Join-Path $output 'InventorScreenshotTool.deps.json'
if (Test-Path $deps) { Copy-Item $deps $stage }
if (Test-Path (Join-Path $stage 'Autodesk.Inventor.Interop.dll')) {
    throw 'ERROR: An Autodesk binary has entered the distributable payload.'
}
Write-Host "Staged application-only files in $stage"
Get-ChildItem $stage | ForEach-Object { Write-Host "  $($_.Name)" }
