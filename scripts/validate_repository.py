"""Portable source and metadata validation; not a replacement for a Windows build."""
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "src/InventorScreenshotTool"
project = ET.parse(SRC / "InventorScreenshotTool.csproj").getroot()
version = project.findtext("./PropertyGroup/Version")
framework = project.findtext("./PropertyGroup/TargetFramework")
assert re.fullmatch(r"\d+\.\d+\.\d+", version or ""), "version missing or invalid"
assert framework == "net8.0-windows", "Inventor 2025.2.1 must target .NET 8"
manifest = ET.parse(SRC / "InventorScreenshotTool.addin").getroot()
server = (SRC / "StandardAddInServer.cs").read_text()
manifest_guid = manifest.findtext("ClassId").strip("{}").upper()
assert manifest_guid in server.upper(), "add-in identity must remain stable"
assert (SRC / "PreviewCanvas.cs").is_file(), "Missing centred preview control"
assert (ROOT / "installer/BetterScreenshot.iss").is_file(), "Missing installer script"
assert "PrivilegesRequired=lowest" in (ROOT / "installer/BetterScreenshot.iss").read_text()
assert "Private>false" in (SRC / "InventorScreenshotTool.csproj").read_text(), "Do not redistribute interop"
assert "{userappdata}\\Autodesk\\Inventor 2025\\Addins\\InventorScreenshotTool" in (ROOT / "installer/BetterScreenshot.iss").read_text()
assert not list(ROOT.rglob("Autodesk.Inventor.Interop.dll")), "Proprietary Autodesk DLL found in repository"
assert not list(ROOT.rglob("*.nupkg")), "Unexpected NuGet package found in repository"
# Ribbon PNG IHDR colour type 2 means opaque RGB (not RGBA colour type 6).
for size in (16, 32):
    png = (SRC / f"Assets/cube-focus-{size}.png").read_bytes()
    assert png[:8] == b"\x89PNG\r\n\x1a\n", f"Invalid PNG at {size}px"
    assert png[25] == 2, f"Ribbon PNG {size}px must be opaque RGB (PNG colour type 2)"
print(f"PASS: Source structure, manifest ID, license safeguards, framework {framework}, version {version}")
print("NOTE: The add-in is NOT compiled in this validation; a Windows Inventor runner is required.")
